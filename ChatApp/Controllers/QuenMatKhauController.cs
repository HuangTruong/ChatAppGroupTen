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

        // ================== HÀM MỚI: TẠO OTP CHỈ TỪ EMAIL ==================
        // Dùng khi form chỉ cho người dùng nhập email, không nhập tài khoản.
        public async Task<string> TaoVaLuuOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // 1) Tìm tài khoản trong Firebase theo email
            string taiKhoan = await _authService.GetAccountByEmailAsync(email);
            if (string.IsNullOrWhiteSpace(taiKhoan))
            {
                // Không có user nào trùng email
                return null;
            }

            // 2) Tái sử dụng hàm cũ: kiểm tra + tạo OTP + lưu vào Firebase
            return await TaoVaLuuOtpAsync(taiKhoan, email);
        }

        // ====== HÀM: FORM GỌI ĐỂ LẤY TÀI KHOẢN TỪ EMAIL ======
        public Task<string> TimTaiKhoanBangEmailAsync(string email)
        {
            // chỉ wrap lại cho gọn, sau này nếu đổi logic thì chỉnh 1 nơi
            return _authService.GetAccountByEmailAsync(email);
        }

        // ================== HÀM CŨ: GIỮ LẠI CHO TƯƠNG THÍCH ==================
        // Tạo và lưu mã OTP mới (khi đã biết sẵn tài khoản + email)
        public async Task<string> TaoVaLuuOtpAsync(string taiKhoan, string email)
        {
            // Kiểm tra tài khoản + email có hợp lệ
            bool hopLe = await _authService.IsAccountEmailAsync(taiKhoan, email);
            if (!hopLe) return null;

            string maOtp = new Random().Next(100000, 999999).ToString();
            DateTime hetHan = DateTime.UtcNow.AddMinutes(5);

            var otpInfo = new ThongTinMaFirebase
            {
                Ma = maOtp,
                HetHanLuc = hetHan.ToString("o")
            };

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
