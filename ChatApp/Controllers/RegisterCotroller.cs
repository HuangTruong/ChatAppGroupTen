using ChatApp.Models.Users;
using ChatApp.Services;
using ChatApp.Services.Auth;
using System;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    public class RegisterController
    {
        private readonly AuthService _authService = new AuthService();

        // Logic đăng ký chính
        public async Task DangKyAsync(User user, string xacNhanMatKhau)
        {
            // 1. Kiểm tra đủ thông tin
            if (string.IsNullOrWhiteSpace(user.TaiKhoan) ||
                string.IsNullOrWhiteSpace(user.MatKhau) ||
                string.IsNullOrWhiteSpace(xacNhanMatKhau) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Ten) ||
                string.IsNullOrWhiteSpace(user.Ngaysinh) ||
                string.IsNullOrWhiteSpace(user.Gioitinh))
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin!");
            }

            // 2. Xác nhận mật khẩu
            if (user.MatKhau != xacNhanMatKhau)
                throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp!");

            // 3. Trùng tài khoản
            if (await _authService.GetUserAsync(user.TaiKhoan) != null)
                throw new Exception("Tên tài khoản đã tồn tại!");

            // 4. Trùng email
            if (await _authService.EmailExistsAsync(user.Email))
                throw new Exception("Email đã tồn tại!");


            // 6. Đăng ký lên Firebase
            await _authService.RegisterAsync(user);
        }


        // Check email đã tồn tại chưa (dùng trước khi gửi OTP)
        public async Task<bool> KiemTraEmailTonTaiAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();
            return await _authService.EmailExistsAsync(email);
        }

        // Check tài khoản đã tồn tại chưa (dùng trước khi gửi OTP)
        public async Task<bool> KiemTraTaiKhoanTonTaiAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return false;

            taiKhoan = taiKhoan.Trim();
            var user = await _authService.GetUserAsync(taiKhoan);
            return user != null;
        }
    }
}
