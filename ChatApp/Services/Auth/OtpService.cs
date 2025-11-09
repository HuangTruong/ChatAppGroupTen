using FireSharp.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

using ChatApp.Helpers;
using ChatApp.Models.Otp;
using ChatApp.Services.Firebase;

namespace ChatApp.Services.Auth

{
    public class OtpService
    {
        private readonly IFirebaseClient _client = FirebaseClientFactory.Create();
        // Lưu thông tin mã OTP của một tài khoản lên Firebase.
        public async Task SaveOtpAsync(string taiKhoan, ThongTinMaFirebase otp)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.SetAsync($"otp/{key}", otp);
        }

        // Lấy mã OTP và thời gian hết hạn của một tài khoản từ Firebase.
        public async Task<ThongTinMaFirebase> GetOtpAsync(string taiKhoan)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            var res = await _client.GetAsync($"otp/{key}");
            return res.Body == "null" ? null : res.ResultAs<ThongTinMaFirebase>();
        }

        // Xoá mã OTP của một tài khoản khỏi Firebase sau khi OTP đã được dùng hoặc hết hạn.
        public async Task DeleteOtpAsync(string taiKhoan)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            await _client.DeleteAsync($"otp/{key}");
        }

        // Gửi mã OTP qua email
        public void GuiEmailOtp(string emailNhan, string ma)
        {
            try
            {
                string emailGui = "hnhom17@gmail.com";
                string matKhauUngDung = "gcgq xzja ivub klbo"; // app password
                string tieuDe = "Mã xác nhận đổi mật khẩu";
                string noiDung = $"Xin chào,\n\nMã xác nhận của bạn là: {ma}\nMã có hiệu lực trong 5 phút.\n\nTrân trọng!";

                using (MailMessage mail = new MailMessage(emailGui, emailNhan, tieuDe, noiDung))
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(emailGui, matKhauUngDung);
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email thất bại: " + ex.Message);
            }
        }
    }
}
