using System;
using System.Threading.Tasks;

using ChatApp.Models.Otp;
using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    public class ForgotPasswordController
    {
        private readonly AuthService _authService = new AuthService();
        private readonly OtpService _otpService = new OtpService();

        // Tạo và lưu mã OTP mới
        public async Task<string> TaoVaLuuOtpAsync(string taiKhoan, string email)
        {
            // Kiểm tra tài khoản + email có hợp lệ
            bool hopLe = await _authService.IsAccountEmailAsync(taiKhoan, email);
            if (!hopLe) return null;

            string maOtp = new Random().Next(100000, 999999).ToString();
            DateTime hetHan = DateTime.UtcNow.AddMinutes(5);

            var otpInfo = new ThongTinMaFirebase { Ma = maOtp, HetHanLuc = hetHan.ToString("o") };
            await _otpService.SaveOtpAsync(taiKhoan, otpInfo);

            return maOtp;
        }

        // Kiểm tra mã OTP hợp lệ
        public async Task<bool> KiemTraOtpHopLeAsync(string taiKhoan, string maNhap)
        {
            var otp = await _otpService.GetOtpAsync(taiKhoan);
            if (otp == null) return false;

            if (!DateTime.TryParse(otp.HetHanLuc, out DateTime hetHan))
                return false;

            bool hopLe = DateTime.UtcNow <= hetHan && otp.Ma == maNhap;

            // Nếu OTP đã xác nhận thành công hoặc đã hết hạn thì xóa luôn
            if (hopLe || DateTime.UtcNow > hetHan)
            {
                await _otpService.DeleteOtpAsync(taiKhoan);
            }

            return hopLe;
        }
        // Gửi OTP qua email
        public void GuiEmailOtp(string emailNhan, string maOtp)
        {
            _otpService.GuiEmailOtp(emailNhan, maOtp);
        }
    }
}
