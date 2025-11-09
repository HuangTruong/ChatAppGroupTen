using FireSharp.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

using ChatApp.Helpers;
using ChatApp.Models.Users;

namespace ChatApp.Services.Auth
{
    public class AuthService
    {
        private readonly IFirebaseClient _firebase;

        // ✅ Constructor: nếu không truyền vào thì tự tạo bằng FirebaseClientFactory
        public AuthService(IFirebaseClient firebase = null)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // ✅ Lấy thông tin người dùng theo tài khoản
        public async Task<User> GetUserAsync(string taiKhoan)
        {
            var res = await _firebase.GetAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}");
            return res.Body == "null" ? null : res.ResultAs<User>();
        }

        // ✅ Kiểm tra username đã tồn tại chưa
        public async Task<bool> UsernameExistsAsync(string ten)
        {
            var res = await _firebase.GetAsync($"Username/{ten}");
            return res.Body != "null";
        }

        // ✅ Kiểm tra email đã được đăng ký chưa
        public async Task<bool> EmailExistsAsync(string email)
        {
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            var res = await _firebase.GetAsync($"emails/{enc}");
            return res.Body != "null";
        }

        // ✅ Kiểm tra tài khoản có khớp email không
        public async Task<bool> IsAccountEmailAsync(string taiKhoan, string email)
        {
            var user = await GetUserAsync(taiKhoan);
            if (user == null) return false;
            return string.Equals(user.Email?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // ✅ Đăng ký tài khoản mới
        public async Task RegisterAsync(User user)
        {
            string safeKey = KeySanitizer.SafeKey(user.TaiKhoan);
            string encEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));

            await _firebase.SetAsync($"users/{safeKey}", user);
            await _firebase.SetAsync($"emails/{encEmail}", true);
            await _firebase.SetAsync($"Username/{user.Ten}", true);
        }

        // ✅ Đổi mật khẩu
        public async Task UpdatePasswordAsync(string taiKhoan, string mkMoi)
        {
            await _firebase.UpdateAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}", new { MatKhau = mkMoi });
        }

        // ✅ Cập nhật trạng thái người dùng (Online, Offline, Typing...)
        public async Task UpdateStatusAsync(string taiKhoan, string trangThai)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            await _firebase.SetAsync($"status/{key}", trangThai);
        }
    }
}
