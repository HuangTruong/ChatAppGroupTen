using System;
using System.Threading.Tasks;
using ChatApp.Models.Users;
using ChatApp.Services.Auth;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    public class LoginController
    {
        private readonly AuthService _authService;

        public LoginController()
        {
            var client = FirebaseClientFactory.Create();
            _authService = new AuthService(client);
        }

        public async Task<User> DangNhapAsync(string taiKhoan, string matKhau)
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

            // Đăng nhập thành công -> cập nhật trạng thái ONLINE
            await _authService.UpdateStatusAsync(user.Ten, "online");

            return user;
        }
    }
}
