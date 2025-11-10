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

        // Xử lý logic đăng ký người dùng
        public async Task DangKyAsync(User user, string xacNhanMatKhau)
        {
            // Kiểm tra đủ thông tin
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

            // Kiểm tra xác nhận mật khẩu
            if (user.MatKhau != xacNhanMatKhau)
                throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp!");

            // Kiểm tra trùng tài khoản
            if (await _authService.GetUserAsync(user.TaiKhoan) != null)
                throw new Exception("Tên tài khoản đã tồn tại!");

            // Kiểm tra trùng email
            if (await _authService.EmailExistsAsync(user.Email))
                throw new Exception("Email đã tồn tại!");

            // Kiểm tra trùng tên hiển thị
            if (await _authService.UsernameExistsAsync(user.Ten))
                throw new Exception("Tên hiển thị đã tồn tại!");

            // Đăng ký tài khoản lên Firebase
            await _authService.RegisterAsync(user);
        }

        // Hàm hỗ trợ: dùng cho DangKy để CHECK email trước khi gửi OTP
        public async Task<bool> KiemTraEmailTonTaiAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();
            return await _authService.EmailExistsAsync(email);
        }
    }
}
