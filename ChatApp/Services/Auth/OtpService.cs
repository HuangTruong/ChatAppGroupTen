using System;
using System.Threading.Tasks;
using FireSharp.Interfaces;

using ChatApp.Helpers;
using ChatApp.Models.Otp;
using ChatApp.Services.Firebase;
using ChatApp.Services.Email;

namespace ChatApp.Services.Auth
{
    public class OtpService
    {
        private readonly IFirebaseClient _client;

        // Dùng mặc định: tự tạo FirebaseClient + SmtpEmailSender
        public OtpService() {
            _client = FirebaseClientFactory.Create();
        }

        // Cho phép truyền từ ngoài vào (nếu sau này muốn DI / test)
        public OtpService(IFirebaseClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        // Lưu thông tin mã OTP của một tài khoản lên Firebase.
        public async Task SaveOtpAsync(string taiKhoan, ThongTinMaFirebase otp)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                throw new ArgumentException("Tài khoản không hợp lệ.", nameof(taiKhoan));

            if (otp == null)
                throw new ArgumentNullException(nameof(otp));

            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.SetAsync($"otp/{key}", otp);
        }

        // Lấy mã OTP và thời gian hết hạn của một tài khoản từ Firebase.
        public async Task<ThongTinMaFirebase> GetOtpAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return null;

            string key = KeySanitizer.SafeKey(taiKhoan);
            var res = await _client.GetAsync($"otp/{key}");
            return res.Body == "null" ? null : res.ResultAs<ThongTinMaFirebase>();
        }

        // Xoá mã OTP của một tài khoản khỏi Firebase sau khi OTP đã được dùng hoặc hết hạn.
        public async Task DeleteOtpAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return;

            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.DeleteAsync($"otp/{key}");
        }

        // Gửi mã OTP qua email (dùng chung hạ tầng IEmailSender)
        // Giữ nguyên kiểu gọi sync để không phải sửa Controller/Form
        public void GuiEmailOtp(string emailNhan, string ma)
        {
            if (string.IsNullOrWhiteSpace(emailNhan))
                throw new ArgumentException("Email nhận không hợp lệ.", nameof(emailNhan));

            if (string.IsNullOrWhiteSpace(ma))
                throw new ArgumentException("Mã OTP không hợp lệ.", nameof(ma));

            try
            {
                string subject = "Mã xác nhận đổi mật khẩu ChatApp";

                // Body HTML, style na ná EmailVerificationService cho đồng bộ
                string body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif'>
    <h2>Mã xác nhận đổi mật khẩu ChatApp</h2>
    <p>Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu cho tài khoản ChatApp của bạn.</p>
    <p>Mã OTP của bạn là:</p>
    <div style='font-size:26px;font-weight:bold;letter-spacing:3px'>{ma}</div>
    <p>Mã có hiệu lực trong 5 phút.</p>
    <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
</div>";

                // Gọi async nhưng block lại cho đơn giản (Form đang dùng void)
                var sender = new SmtpEmailSender();
                sender
                    .SendEmailAsync(emailNhan, subject, body)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                // Ném lỗi rõ ràng hơn, giúp debug
                throw new Exception("Gửi email OTP thất bại: " + ex.Message, ex);
            }
        }
    }
}
