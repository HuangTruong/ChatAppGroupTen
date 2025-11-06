using System;
using System.Text;
using System.Threading.Tasks;
using FireSharp.Response;

using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;



namespace ChatApp.Services.Auth
{
    public class AuthService
    {
        // Lấy thông tin của một người dùng từ Firebase theo tài khoản (taiKhoan).
        public async Task<UserDto> GetUserAsync(string taiKhoan)
        {
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}");
            return res.Body == "null" ? null : res.ResultAs<UserDto>();
        }

        //Kiểm tra xem tên hiển thị(username) đã tồn tại chưa.
        public async Task<bool> UsernameExistsAsync(string ten)
        {
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"Username/{ten}");
            return res.Body != "null";
        }

        // Kiểm tra xem email đã được đăng ký chưa.
        public async Task<bool> EmailExistsAsync(string email)
        {
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"emails/{enc}");
            return res.Body != "null";
        }

        // Đăng ký một tài khoản mới.
        public async Task RegisterAsync(UserDK user)
        {
            var client = FirebaseClientFactory.Create();
            await client.SetAsync($"users/{KeySanitizer.SafeKey(user.TaiKhoan)}", user);
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));
            await client.SetAsync($"emails/{enc}", true);
            await client.SetAsync($"Username/{user.Ten}", true);
        }

        // Đổi mật khẩu cho tài khoản.
        public async Task UpdatePasswordAsync(string taiKhoan, string mkMoi)
        {
            var client = FirebaseClientFactory.Create();
            await client.UpdateAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}", new { MatKhau = mkMoi });
        }
    }
}
