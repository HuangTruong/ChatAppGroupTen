using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Services.Email;

namespace ChatApp.Services.Auth
{
    /// <summary>
    /// Service xác thực email tạm thời trong bộ nhớ:
    /// - Tạo mã 6 số ngẫu nhiên (OTP) an toàn.
    /// - Giới hạn thời gian sống của mã.
    /// - Giới hạn thời gian gửi lại (cooldown).
    /// - Giới hạn số lần nhập sai.
    /// </summary>
    public static class EmailVerificationService
    {
        #region ======== Kiểu dữ liệu nội bộ ========

        /// <summary>
        /// Thông tin lưu trữ cho từng email:
        /// mã hiện tại, thời gian hết hạn, thời gian gửi gần nhất, số lần nhập.
        /// </summary>
        private class Entry
        {
            /// <summary>
            /// Mã xác thực (6 chữ số).
            /// </summary>
            public string Code { get; set; } = string.Empty;

            /// <summary>
            /// Thời điểm mã hết hạn (UTC).
            /// </summary>
            public DateTime ExpireAt { get; set; }

            /// <summary>
            /// Thời điểm gần nhất mã được gửi (UTC) dùng để tính cooldown.
            /// </summary>
            public DateTime LastSentAt { get; set; }

            /// <summary>
            /// Số lần người dùng đã thử nhập mã.
            /// </summary>
            public int Attempts { get; set; } = 0;
        }

        #endregion

        #region ======== Store & Hằng số cấu hình ========

        /// <summary>
        /// Store in-memory: key = email, value = Entry tương ứng.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Entry> _store =
            new ConcurrentDictionary<string, Entry>();

        /// <summary>
        /// Thời gian sống của mã (phút).
        /// </summary>
        private const int ExpireMinutes = 5;

        /// <summary>
        /// Thời gian cooldown (giây) trước khi cho phép gửi lại mã tới cùng email.
        /// </summary>
        private const int ResendCooldownSeconds = 60;

        /// <summary>
        /// Số lần nhập sai tối đa trước khi buộc phải gửi lại mã.
        /// </summary>
        private const int MaxAttempts = 10;

        #endregion

        #region ======== Sinh mã xác thực ========

        /// <summary>
        /// Sinh mã 6 chữ số ngẫu nhiên an toàn dùng <see cref="RandomNumberGenerator"/>.
        /// </summary>
        /// <returns>Chuỗi 6 chữ số, có thể có số 0 ở đầu.</returns>
        private static string GenerateCode()
        {
            var bytes = new byte[4];

            // Tạo số ngẫu nhiên an toàn bằng RandomNumberGenerator
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            uint val = BitConverter.ToUInt32(bytes, 0) % 1000000u; // 0..999999
            return val.ToString("D6"); // format thành 6 số, đủ 0 phía trước
        }

        #endregion

        #region ======== Kiểm tra cooldown gửi lại mã ========

        /// <summary>
        /// Kiểm tra email có được phép gửi lại mã hay không,
        /// dựa trên thời gian cooldown.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <param name="waitSeconds">
        /// Nếu không được gửi, trả về số giây còn phải chờ.
        /// </param>
        /// <returns>
        /// True nếu được phép gửi lại ngay, False nếu vẫn đang trong thời gian cooldown.
        /// </returns>
        public static bool CanResend(string email, out int waitSeconds)
        {
            waitSeconds = 0;

            if (_store.TryGetValue(email, out var e))
            {
                var remain = (int)Math.Ceiling(
                    (e.LastSentAt.AddSeconds(ResendCooldownSeconds) - DateTime.UtcNow)
                    .TotalSeconds);

                if (remain > 0)
                {
                    waitSeconds = remain;
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ======== Gửi mã mới qua email ========

        /// <summary>
        /// Tạo mã mới, lưu vào store và gửi qua email sử dụng <see cref="IEmailSender"/>.
        /// </summary>
        /// <param name="email">Email người nhận.</param>
        /// <param name="sender">Service gửi email.</param>
        public static async Task SendNewCodeAsync(string email, IEmailSender sender)
        {
            var code = GenerateCode();

            _store.AddOrUpdate(
                email,
                delegate
                {
                    return new Entry
                    {
                        Code = code,
                        ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes),
                        LastSentAt = DateTime.UtcNow,
                        Attempts = 0
                    };
                },
                delegate (string _, Entry old)
                {
                    old.Code = code;
                    old.ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes);
                    old.LastSentAt = DateTime.UtcNow;
                    old.Attempts = 0;
                    return old;
                });

            var html = new StringBuilder()
                .Append("<div style='font-family:Segoe UI,Arial,sans-serif'>")
                .Append("<h2>Mã xác nhận đăng ký ChatApp</h2>")
                .Append("<p>Mã của bạn là:</p>")
                .Append("<div style='font-size:26px;font-weight:bold;letter-spacing:3px'>")
                .Append(code)
                .Append("</div>")
                .Append("<p>Mã có hiệu lực trong 5 phút.</p>")
                .Append("</div>")
                .ToString();

            await sender.SendEmailAsync(email, "Mã xác nhận đăng ký ChatApp", html);
        }

        #endregion

        #region ======== Xác minh mã ========

        /// <summary>
        /// Xác minh mã người dùng nhập vào:
        /// - Kiểm tra đã từng gửi mã cho email này chưa.
        /// - Kiểm tra hết hạn.
        /// - Kiểm tra vượt quá số lần thử.
        /// - So khớp chính xác mã.
        /// </summary>
        /// <param name="email">Email đã được gửi mã.</param>
        /// <param name="code">Mã người dùng nhập.</param>
        /// <param name="error">
        /// Thông báo lỗi thân thiện cho người dùng nếu trả về False.
        /// </param>
        /// <returns>True nếu mã hợp lệ, ngược lại False.</returns>
        public static bool Verify(string email, string code, out string error)
        {
            error = string.Empty;

            // Chưa gửi mã
            if (!_store.TryGetValue(email, out var e))
            {
                error = "Chưa gửi mã tới email này. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            // Hết hạn
            if (DateTime.UtcNow > e.ExpireAt)
            {
                error = "Mã đã hết hạn. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            // Vượt quá số lần thử
            if (e.Attempts >= MaxAttempts)
            {
                error = "Thử quá nhiều lần. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            e.Attempts++;

            // Sai mã
            if (!string.Equals(e.Code, code == null ? null : code.Trim(), StringComparison.Ordinal))
            {
                error = "Mã không đúng.";
                return false;
            }

            // Đúng mã: xóa entry để tránh reuse
            _store.TryRemove(email, out _);
            return true;
        }

        #endregion
    }
}
