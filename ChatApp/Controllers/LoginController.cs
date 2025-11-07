using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Services.Auth;
using ChatApp.Models.Users;

namespace ChatApp.Controllers
{
    public class LoginController
    {
        private readonly AuthService _authService = new AuthService();

        // Hàm xử lý logic đăng nhập
        public async Task<UserDto> DangNhapAsync(string taiKhoan, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                throw new ArgumentException("Vui lòng nhập tên đăng nhập!");
            if (string.IsNullOrWhiteSpace(matKhau))
                throw new ArgumentException("Vui lòng nhập mật khẩu!");

            var user = await _authService.GetUserAsync(taiKhoan);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại!");
            if (user.MatKhau != matKhau)
                throw new InvalidOperationException("Mật khẩu không đúng!");

            return user;
        }
    }
}
