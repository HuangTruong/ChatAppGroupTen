using ChatApp.Models.Users;
using ChatApp.Services;
using ChatApp.Services.Auth;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ChatApp.Controllers
{
    public class DangKyController
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

            // 5. Kiểm tra email có hợp lệ không
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (string.IsNullOrWhiteSpace(user.Email) || !Regex.IsMatch(user.Email, pattern))
            {
                throw new Exception("Định dạng email không hợp lệ.");
            }

            // 6. Đăng ký lên Firebase
            await _authService.RegisterAsync(user);
        }
    }
}
