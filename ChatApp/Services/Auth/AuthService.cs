using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.Status;     
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services.Auth
{
    public class AuthService
    {
        private readonly IFirebaseClient _firebase;
        private readonly StatusService _statusService;   

        // Constructor: nếu không truyền vào thì tự tạo bằng FirebaseClientFactory
        public AuthService(IFirebaseClient firebase = null)
        {
            _firebase = firebase ?? FirebaseClientFactory.Create();
            _statusService = new StatusService(_firebase);    
        }

        // Lấy thông tin người dùng theo tài khoản
        public async Task<User> GetUserAsync(string taiKhoan)
        {
            var res = await _firebase.GetAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}");
            return res.Body == "null" ? null : res.ResultAs<User>();
        }

        // Tìm tài khoản (User.TaiKhoan) theo email
        public async Task<string> GetAccountByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Lấy toàn bộ danh sách users
            var res = await _firebase.GetAsync("users");
            if (res.Body == "null")
                return null;

            var usersDict = res.ResultAs<Dictionary<string, User>>();
            if (usersDict == null)
                return null;

            foreach (var kvp in usersDict)
            {
                var user = kvp.Value;
                if (user?.Email == null) continue;

                if (string.Equals(user.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // Trả về tài khoản thật của user
                    return user.TaiKhoan;
                }
            }

            // Không tìm thấy user có email này
            return null;
        }

        // Kiểm tra email đã được đăng ký chưa
        public async Task<bool> EmailExistsAsync(string email)
        {
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            var res = await _firebase.GetAsync($"emails/{enc}");
            return res.Body != "null";
        }

        // Kiểm tra tài khoản có khớp email không
        public async Task<bool> IsAccountEmailAsync(string taiKhoan, string email)
        {
            var user = await GetUserAsync(taiKhoan);
            if (user == null) return false;
            return string.Equals(user.Email?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Đăng ký tài khoản mới
        public async Task RegisterAsync(User user)
        {
            string safeKey = KeySanitizer.SafeKey(user.TaiKhoan);
            string encEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));

            await _firebase.SetAsync($"users/{safeKey}", user);
            await _firebase.SetAsync($"emails/{encEmail}", true);
        }

        // Đổi mật khẩu
        public async Task UpdatePasswordAsync(string taiKhoan, string mkMoi)
        {
            await _firebase.UpdateAsync($"users/{KeySanitizer.SafeKey(taiKhoan)}", new { MatKhau = mkMoi });
        }

        // ===== Avatar lưu base64 trong users/{key}/avatar =====
        public async Task<string> GetAvatarAsync(string taiKhoan)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            var res = await _firebase.GetAsync($"users/{key}/avatar");
            return res.Body == "null" ? null : res.ResultAs<string>();
        }

        public async Task UpdateAvatarAsync(string taiKhoan, string avatarBase64)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            await _firebase.SetAsync($"users/{key}/avatar", avatarBase64);
        }

        // ===== Đổi email (cập nhật cả bảng emails) =====
        public async Task UpdateEmailAsync(string taiKhoan, string emailMoi)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan) || string.IsNullOrWhiteSpace(emailMoi))
                throw new ArgumentException("Tài khoản hoặc email mới không hợp lệ.");

            var user = await GetUserAsync(taiKhoan);
            if (user == null)
                throw new InvalidOperationException("Không tìm thấy người dùng.");

            string safeKey = KeySanitizer.SafeKey(taiKhoan);

            // Xóa mapping email cũ (nếu có)
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                string encOld = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));
                await _firebase.DeleteAsync($"emails/{encOld}");
            }

            // Cập nhật email mới trong users
            await _firebase.UpdateAsync($"users/{safeKey}", new { Email = emailMoi });

            // Thêm mapping email mới
            string encNew = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailMoi));
            await _firebase.SetAsync($"emails/{encNew}", true);
        }

        // Cập nhật trạng thái người dùng (Online, Offline, Typing...)

        public Task UpdateStatusAsync(string taiKhoan, string trangThai)
        {
            return _statusService.UpdateAsync(taiKhoan, trangThai);
        }
    }
}
