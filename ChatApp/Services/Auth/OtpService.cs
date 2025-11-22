using System;
using System.Threading.Tasks;
using FireSharp.Interfaces;
using ChatApp.Helpers;
using ChatApp.Models.Otp;
using ChatApp.Services.Firebase;
using ChatApp.Services.Email;

namespace ChatApp.Services.Auth
{
    /// <summary>
    /// Service quản lý OTP (mã xác thực 1 lần) cho chức năng quên mật khẩu:
    /// - Lưu / đọc / xoá OTP trên Firebase.
    /// - Gửi email OTP thông qua <see cref="IEmailSender"/>.
    /// </summary>
    public class OtpService
    {
        #region ======== Trường / Dependencies ========

        /// <summary>
        /// Client Firebase dùng để lưu OTP vào Realtime Database.
        /// </summary>
        private readonly IFirebaseClient _client;

        /// <summary>
        /// Thành phần gửi email (SMTP hoặc implementation khác) cho OTP.
        /// </summary>
        private readonly IEmailSender _emailSender;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo mặc định:
        /// - Tự tạo <see cref="IFirebaseClient"/> bằng <see cref="FirebaseClientFactory.Create"/>.
        /// - Tự tạo <see cref="SmtpEmailSender"/> để gửi email.
        /// </summary>
        public OtpService()
            : this(FirebaseClientFactory.Create(), new SmtpEmailSender())
        {
        }

        /// <summary>
        /// Khởi tạo với dependency truyền từ ngoài vào
        /// (giúp dễ test / DI về sau).
        /// </summary>
        /// <param name="client">Client Firebase dùng để lưu OTP.</param>
        /// <param name="emailSender">Service gửi email OTP.</param>
        public OtpService(IFirebaseClient client, IEmailSender emailSender)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (emailSender == null) throw new ArgumentNullException("emailSender");

            _client = client;
            _emailSender = emailSender;
        }

        #endregion

        #region ======== Lưu / Lấy / Xoá OTP trên Firebase ========

        /// <summary>
        /// Lưu thông tin mã OTP của một tài khoản lên Firebase tại node <c>otp/{key}</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần gắn OTP.</param>
        /// <param name="otp">Thông tin mã OTP (mã + thời gian hết hạn).</param>
        public async Task SaveOtpAsync(string taiKhoan, ThongTinMaFirebase otp)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                throw new ArgumentException("Tài khoản không hợp lệ.", "taiKhoan");

            if (otp == null)
                throw new ArgumentNullException("otp");

            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.SetAsync("otp/" + key, otp);
        }

        /// <summary>
        /// Lấy thông tin mã OTP (nếu có) của một tài khoản từ Firebase.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần đọc OTP.</param>
        /// <returns>
        /// <see cref="ThongTinMaFirebase"/> nếu tồn tại, ngược lại <c>null</c>.
        /// </returns>
        public async Task<ThongTinMaFirebase> GetOtpAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return null;

            string key = KeySanitizer.SafeKey(taiKhoan);
            var res = await _client.GetAsync("otp/" + key);
            return res.Body == "null" ? null : res.ResultAs<ThongTinMaFirebase>();
        }

        /// <summary>
        /// Xoá OTP của một tài khoản khỏi Firebase (dùng khi đã verify xong hoặc hết hạn).
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần xoá OTP.</param>
        public async Task DeleteOtpAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return;

            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.DeleteAsync("otp/" + key);
        }

        #endregion

        #region ======== Gửi email OTP ========

        /// <summary>
        /// Gửi mã OTP qua email để xác nhận đổi mật khẩu.
        /// Dùng <see cref="IEmailSender"/> nội bộ, gọi async nhưng block lại
        /// để tương thích với các Form/Controller đang dùng kiểu void.
        /// </summary>
        /// <param name="emailNhan">Địa chỉ email nhận OTP.</param>
        /// <param name="ma">Mã OTP cần gửi.</param>
        public void GuiEmailOtp(string emailNhan, string ma)
        {
            if (string.IsNullOrWhiteSpace(emailNhan))
                throw new ArgumentException("Email nhận không hợp lệ.", "emailNhan");

            if (string.IsNullOrWhiteSpace(ma))
                throw new ArgumentException("Mã OTP không hợp lệ.", "ma");

            try
            {
                string subject = "Mã xác nhận đổi mật khẩu ChatApp";

                // Body HTML (style tương đồng với EmailVerificationService để đồng bộ UI/UX email).
                string body = @"
<div style='font-family:Segoe UI,Arial,sans-serif'>
    <h2>Mã xác nhận đổi mật khẩu ChatApp</h2>
    <p>Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu cho tài khoản ChatApp của bạn.</p>
    <p>Mã OTP của bạn là:</p>
    <div style='font-size:26px;font-weight:bold;letter-spacing:3px'>" + ma + @"</div>
    <p>Mã có hiệu lực trong 5 phút.</p>
    <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
</div>";

                // Gọi async nhưng block lại cho đơn giản (Form đang dùng void, chưa async).
                _emailSender
                    .SendEmailAsync(emailNhan, subject, body)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                // Ném lỗi rõ ràng hơn để debug / hiển thị log.
                throw new Exception("Gửi email OTP thất bại: " + ex.Message, ex);
            }
        }

        #endregion
    }
}
