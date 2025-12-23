using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Services.Email;

namespace ChatApp.Services.Auth
{
    /// <summary>
    /// Dịch vụ xác thực email:
    /// - Tạo và lưu mã xác nhận theo email.
    /// - Gửi mã qua email.
    /// - Kiểm soát thời gian sống của mã, cooldown gửi lại, số lần nhập sai.
    /// - Xác minh mã người dùng nhập.
    /// </summary>
    public static class EmailVerificationService
    {
        #region ====== STORAGE & CONFIG ======

        /// <summary>
        /// Entry lưu thông tin mã xác nhận cho một email.
        /// </summary>
        private class Entry
        {
            /// <summary>
            /// Mã xác nhận (6 chữ số).
            /// </summary>
            public string Code { get; set; } = string.Empty;

            /// <summary>
            /// Thời điểm mã hết hạn.
            /// </summary>
            public DateTime ExpireAt { get; set; }

            /// <summary>
            /// Thời điểm gần nhất gửi mã đến email này.
            /// Dùng để kiểm soát cooldown gửi lại mã.
            /// </summary>
            public DateTime LastSentAt { get; set; }

            /// <summary>
            /// Số lần đã thử nhập mã (để chặn brute-force).
            /// </summary>
            public int Attempts { get; set; } = 0;
        }

        /// <summary>
        /// Bộ nhớ lưu tạm mã xác nhận theo email.
        /// Key: email, Value: thông tin mã và trạng thái.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Entry> _store =
            new ConcurrentDictionary<string, Entry>();

        /// <summary>
        /// Thời gian sống của mã xác nhận (phút).
        /// </summary>
        private const int ExpireMinutes = 5;          // Mã sống 5 phút

        /// <summary>
        /// Thời gian cooldown tối thiểu giữa 2 lần gửi lại mã (giây).
        /// </summary>
        private const int ResendCooldownSeconds = 60; // 60s mới cho gửi lại

        /// <summary>
        /// Số lần nhập sai tối đa trước khi bắt buộc phải gửi mã mới.
        /// </summary>
        private const int MaxAttempts = 10;           // Giới hạn thử sai

        #endregion

        #region ====== TẠO MÃ XÁC NHẬN ======

        /// <summary>
        /// Sinh mã xác nhận ngẫu nhiên dạng 6 chữ số (000000–999999)
        /// bằng bộ sinh số ngẫu nhiên an toàn (RandomNumberGenerator).
        /// </summary>
        /// <returns>Mã xác nhận dạng chuỗi 6 ký tự số.</returns>
        private static string GenerateCode()
        {
            var bytes = new byte[4];

            // Tạo số ngẫu nhiên an toàn
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Ép về khoảng 0..999999
            uint val = BitConverter.ToUInt32(bytes, 0) % 1000000u;

            // Định dạng 6 chữ số, đủ 6 ký tự, thêm 0 ở đầu nếu cần
            return val.ToString("D6");
        }

        #endregion

        #region ====== KIỂM TRA CÓ ĐƯỢC GỬI LẠI MÃ KHÔNG ======

        /// <summary>
        /// Kiểm tra email có được phép gửi lại mã xác nhận hay chưa
        /// dựa trên thời gian cooldown giữa hai lần gửi.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <param name="waitSeconds">
        /// Nếu chưa được gửi lại, trả về số giây còn phải chờ; 
        /// nếu được gửi lại, nhận giá trị 0.
        /// </param>
        /// <returns>
        /// true nếu có thể gửi lại ngay;
        /// false nếu còn đang trong thời gian cooldown.
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

        #region ====== GỬI MÃ TỚI EMAIL ======

        /// <summary>
        /// Sinh mã mới, lưu vào bộ nhớ, và gửi email kèm mã xác nhận đến người dùng.
        /// Thao tác này sẽ reset thời gian sống mã, thời điểm gửi và số lần attempt.
        /// </summary>
        /// <param name="email">Email đích cần gửi mã xác nhận.</param>
        public static async Task SendNewCodeAsync(string email)
        {
            string code = GenerateCode();

            // Lưu mã vào dictionary: thêm mới hoặc ghi đè entry cũ
            _store.AddOrUpdate(
                email,
                // Key mới
                _new => new Entry
                {
                    Code = code,
                    ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes),
                    LastSentAt = DateTime.UtcNow,
                    Attempts = 0
                },
                // Key đã tồn tại
                (_new, old) =>
                {
                    old.Code = code;
                    old.ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes);
                    old.LastSentAt = DateTime.UtcNow;
                    old.Attempts = 0;
                    return old;
                });

            // Tạo nội dung email HTML
            string html = new StringBuilder()
                .Append("<div style='font-family:Segoe UI,Arial,sans-serif'>")
                .Append("<h2>Mã xác nhận đăng ký ChatApp</h2>")
                .Append("<p>Mã của bạn là:</p>")
                .Append("<div style='font-size:26px;font-weight:bold;letter-spacing:3px'>")
                .Append(code)
                .Append("</div>")
                .Append("<p>Mã có hiệu lực trong 5 phút.</p>")
                .Append("</div>")
                .ToString();

            // Gửi email qua SMTP
            var sender = new SmtpEmailSender();
            await sender.SendEmailAsync(
                email,
                "Mã xác nhận đăng ký ChatApp",
                html);
        }

        #endregion

        #region ====== XÁC THỰC MÃ ======

        /// <summary>
        /// Xác thực mã xác nhận mà người dùng nhập vào.
        /// Kiểm tra:
        /// - Đã từng gửi mã cho email này chưa.
        /// - Mã còn hạn không.
        /// - Có vượt quá số lần thử tối đa không.
        /// - Mã nhập có trùng với mã đã lưu không.
        /// </summary>
        /// <param name="email">Email cần xác thực.</param>
        /// <param name="code">Mã người dùng nhập.</param>
        /// <param name="error">
        /// Thông báo lỗi chi tiết nếu xác thực thất bại (chuỗi rỗng nếu thành công).
        /// </param>
        /// <returns>
        /// true nếu mã hợp lệ và xác thực thành công;
        /// false nếu thất bại (lúc này <paramref name="error"/> chứa lý do).
        /// </returns>
        public static bool Verify(string email, string code, out string error)
        {
            error = string.Empty;

            // Chưa có mã cho email này
            if (!_store.TryGetValue(email, out var e))
            {
                error = "Chưa gửi mã tới email này. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            // Mã đã hết hạn
            if (DateTime.UtcNow > e.ExpireAt)
            {
                error = "Mã đã hết hạn. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            // Thử quá nhiều lần
            if (e.Attempts >= MaxAttempts)
            {
                error = "Thử quá nhiều lần. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            // Tăng số lần thử
            e.Attempts++;

            // So sánh mã
            if (!string.Equals(e.Code, code == null ? null : code.Trim(), StringComparison.Ordinal))
            {
                error = "Mã không đúng.";
                return false;
            }

            // Xác thực thành công → xoá entry để tránh tái sử dụng
            _store.TryRemove(email, out _);
            return true;
        }

        #endregion
    }
}
