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

        // ✅ Khởi tạo AuthService với client lấy từ Factory
        public LoginController()
        {
            var client = FirebaseClientFactory.Create();
            _authService = new AuthService(client);
        }

        // Hàm xử lý logic đăng nhập
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

            return user;
        }
    }
}
