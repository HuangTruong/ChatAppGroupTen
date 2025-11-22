using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.Status;
using FireSharp.Interfaces;

namespace ChatApp.Services.Auth
{
    /// <summary>
    /// Service phụ trách toàn bộ logic xác thực / tài khoản người dùng:
    /// - Đăng ký, đổi mật khẩu, đổi email.
    /// - Lưu / đọc avatar.
    /// - Tra cứu user theo tài khoản / email.
    /// - Cập nhật trạng thái online/offline (thông qua <see cref="StatusService"/>).
    /// </summary>
    public class AuthService
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Client Firebase dùng để thao tác Realtime Database.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Service quản lý node trạng thái (online/offline/typing...).
        /// </summary>
        private readonly StatusService _statusService;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo <see cref="AuthService"/>.
        /// Nếu không truyền client vào thì sẽ tự tạo bằng <see cref="FirebaseClientFactory.Create"/>.
        /// </summary>
        /// <param name="firebase">
        /// Client Firebase dùng chung cho service.
        /// Có thể null → sẽ tự khởi tạo.
        /// </param>
        public AuthService(IFirebaseClient firebase = null)
        {
            _firebase = firebase ?? FirebaseClientFactory.Create();
            _statusService = new StatusService(_firebase);
        }

        #endregion

        #region ======== Lấy thông tin người dùng ========

        /// <summary>
        /// Lấy thông tin người dùng theo tài khoản (username) từ node <c>users/{key}</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần truy vấn.</param>
        /// <returns>
        /// Đối tượng <see cref="User"/> nếu tồn tại, ngược lại trả về <c>null</c>.
        /// </returns>
        public async Task<User> GetUserAsync(string taiKhoan)
        {
            var res = await _firebase.GetAsync("users/" + KeySanitizer.SafeKey(taiKhoan));
            return res.Body == "null" ? null : res.ResultAs<User>();
        }

        /// <summary>
        /// Tìm tên tài khoản (<see cref="User.TaiKhoan"/>) dựa trên email.
        /// Duyệt toàn bộ node <c>users</c> để kiểm tra trùng email.
        /// </summary>
        /// <param name="email">Email cần tra cứu.</param>
        /// <returns>
        /// Tên tài khoản nếu tìm thấy, ngược lại <c>null</c>.
        /// </returns>
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
                if (user == null || user.Email == null)
                    continue;

                if (string.Equals(
                        user.Email.Trim(),
                        email.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    // Trả về tài khoản thật của user
                    return user.TaiKhoan;
                }
            }

            // Không tìm thấy user có email này
            return null;
        }

        #endregion

        #region ======== Kiểm tra email / tài khoản ========

        /// <summary>
        /// Kiểm tra email đã được đăng ký hay chưa
        /// thông qua node <c>emails/{base64(email)}</c>.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <returns>
        /// True nếu email đã tồn tại, False nếu chưa được dùng.
        /// </returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            var res = await _firebase.GetAsync("emails/" + enc);
            return res.Body != "null";
        }

        /// <summary>
        /// Kiểm tra tài khoản có khớp với email tương ứng hay không.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần kiểm tra.</param>
        /// <param name="email">Email khai báo.</param>
        /// <returns>
        /// True nếu tài khoản tồn tại và email trùng khớp, ngược lại False.
        /// </returns>
        public async Task<bool> IsAccountEmailAsync(string taiKhoan, string email)
        {
            var user = await GetUserAsync(taiKhoan);
            if (user == null)
                return false;

            return string.Equals(
                user.Email == null ? null : user.Email.Trim(),
                email == null ? null : email.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region ======== Đăng ký tài khoản ========

        /// <summary>
        /// Đăng ký tài khoản mới:
        /// - Lưu user vào node <c>users/{safeKey}</c>.
        /// - Map email vào node <c>emails/{base64(email)}</c> để kiểm tra trùng.
        /// </summary>
        /// <param name="user">Đối tượng người dùng cần đăng ký.</param>
        public async Task RegisterAsync(User user)
        {
            string safeKey = KeySanitizer.SafeKey(user.TaiKhoan);
            string encEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));

            await _firebase.SetAsync("users/" + safeKey, user);
            await _firebase.SetAsync("emails/" + encEmail, true);
        }

        #endregion

        #region ======== Đổi mật khẩu ========

        /// <summary>
        /// Cập nhật mật khẩu mới cho tài khoản:
        /// - Update field <c>MatKhau</c> trong node <c>users/{safeKey}</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần đổi mật khẩu.</param>
        /// <param name="mkMoi">Mật khẩu mới.</param>
        public async Task UpdatePasswordAsync(string taiKhoan, string mkMoi)
        {
            await _firebase.UpdateAsync(
                "users/" + KeySanitizer.SafeKey(taiKhoan),
                new { MatKhau = mkMoi });
        }

        #endregion

        #region ======== Avatar (base64) ========

        /// <summary>
        /// Lấy avatar (chuỗi base64) của tài khoản từ node <c>users/{key}/avatar</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản.</param>
        /// <returns>
        /// Chuỗi base64 ảnh đại diện nếu có, ngược lại <c>null</c>.
        /// </returns>
        public async Task<string> GetAvatarAsync(string taiKhoan)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            var res = await _firebase.GetAsync("users/" + key + "/avatar");
            return res.Body == "null" ? null : res.ResultAs<string>();
        }

        /// <summary>
        /// Cập nhật avatar cho tài khoản ở node <c>users/{key}/avatar</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản.</param>
        /// <param name="avatarBase64">Chuỗi base64 của ảnh đại diện.</param>
        public async Task UpdateAvatarAsync(string taiKhoan, string avatarBase64)
        {
            string key = KeySanitizer.SafeKey(taiKhoan);
            await _firebase.SetAsync("users/" + key + "/avatar", avatarBase64);
        }

        #endregion

        #region ======== Đổi email (kèm mapping emails) ========

        /// <summary>
        /// Cập nhật email mới cho tài khoản:
        /// - Xóa mapping email cũ trong node <c>emails</c> (nếu có).
        /// - Update field Email trong <c>users/{safeKey}</c>.
        /// - Thêm mapping email mới vào node <c>emails</c>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần đổi email.</param>
        /// <param name="emailMoi">Email mới.</param>
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
                await _firebase.DeleteAsync("emails/" + encOld);
            }

            // Cập nhật email mới trong users
            await _firebase.UpdateAsync(
                "users/" + safeKey,
                new { Email = emailMoi });

            // Thêm mapping email mới
            string encNew = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailMoi));
            await _firebase.SetAsync("emails/" + encNew, true);
        }

        #endregion

        #region ======== Trạng thái online / offline ========

        /// <summary>
        /// Cập nhật trạng thái người dùng:
        /// - Gọi xuống <see cref="StatusService"/> để viết vào node status.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản.</param>
        /// <param name="trangThai">Trạng thái mới (vd: "online", "offline").</param>
        /// <returns>Nhiệm vụ bất đồng bộ.</returns>
        public Task UpdateStatusAsync(string taiKhoan, string trangThai)
        {
            return _statusService.UpdateAsync(taiKhoan, trangThai);
        }

        #endregion
    }
}
