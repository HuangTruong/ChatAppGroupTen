using ChatApp.Helpers;
using ChatApp.Models.Users;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Cung cấp các chức năng xác thực với Firebase Auth
    /// và thao tác thông tin người dùng trên Firebase Realtime Database.
    /// </summary>

    public class AuthService
    {
        #region ====== FIELDS & HELPERS ======

        /// <summary>
        /// Dịch vụ HTTP dùng chung để gọi REST API Firebase.
        /// </summary>
        private readonly HttpService _http = new HttpService();

        /// <summary>
        /// Tạo URL truy vấn đến Realtime Database theo path.
        /// </summary>
        /// <param name="path">Đường dẫn con trong DB (ví dụ: users/{localId}).</param>
        /// <returns>URL đầy đủ đến node tương ứng trong Realtime Database.</returns>
        private string Db(string path)
        {
            return string.Format("{0}/{1}.json", FirebaseConfig.DatabaseUrl, path);
        }

        #endregion

        #region ====== AUTH REST URL ======

        /// <summary>
        /// URL đăng ký tài khoản Firebase Auth bằng email/password.
        /// </summary>
        private string SignUpUrl
        {
            get
            {
                return string.Format(
                    "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={0}",
                    FirebaseConfig.ApiKey);
            }
        }

        /// <summary>
        /// URL đăng nhập Firebase Auth bằng email/password.
        /// </summary>
        private string SignInUrl
        {
            get
            {
                return string.Format(
                    "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={0}",
                    FirebaseConfig.ApiKey);
            }
        }

        /// <summary>
        /// URL đổi mật khẩu Firebase Auth.
        /// </summary>
        private string UpdatePasswordUrl
        {
            get
            {
                return string.Format(
                    "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}",
                    FirebaseConfig.ApiKey);
            }
        }

        /// <summary>
        /// URL gửi email reset password (OOB code) của Firebase Auth.
        /// </summary>
        private string ResetPasswordUrl
        {
            get
            {
                return string.Format(
                    "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={0}",
                    FirebaseConfig.ApiKey);
            }
        }

        #endregion

        #region ====== AUTH (LOGIN / REGISTER / CHANGE PASSWORD) ======

        /// <summary>
        /// Đăng nhập Firebase bằng email và password.
        /// </summary>
        /// <param name="email">Email người dùng.</param>
        /// <param name="password">Mật khẩu.</param>
        /// <returns>
        /// Tuple (localId, idToken) do Firebase trả về.
        /// </returns>
        /// <exception cref="Exception">Nếu Firebase trả về lỗi.</exception>
        public async Task<(string localId, string token)> LoginAsync(string email, string password)
        {
            // Gửi request đăng nhập Firebase Auth
            var res = await _http.PostAsync<dynamic>(SignInUrl, new
            {
                email = email,
                password = password,
                returnSecureToken = true
            }).ConfigureAwait(false);

            // Nếu Firebase trả về lỗi
            if (res == null || res.error != null)
            {
                throw new Exception(res != null ? res.error.ToString() : "Firebase sign-in failed.");
            }

            // Lấy dữ liệu Firebase trả về
            string localId = res.localId;
            string token = res.idToken; // Token chuẩn của Firebase

            return (localId, token);
        }
        /// <summary>
        /// Ánh xạ email hoặc username thành email thật lưu trong Firebase.
        /// - Nếu người dùng gõ email (có @) thì trả về nguyên.
        /// - Nếu gõ username thì tìm trong node users theo DisplayName.
        /// </summary>
        public async Task<string> ResolveEmailAsync(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
                return null;

            emailOrUsername = emailOrUsername.Trim();

            // Nếu đã là email (có @) thì trả về luôn
            if (emailOrUsername.Contains("@"))
                return emailOrUsername;

            // Ngược lại: coi như username -> tìm trong /users theo DisplayName
            // Cấu trúc: users/{localId}/DisplayName, Email, ...
            var users = await _http.GetAsync<System.Collections.Generic.Dictionary<string, User>>(
                            Db("users")
                        ).ConfigureAwait(false);

            if (users == null)
                return null;

            foreach (var pair in users)
            {
                var u = pair.Value;
                if (u == null)
                    continue;

                if (!string.IsNullOrEmpty(u.DisplayName) &&
                    string.Equals(u.DisplayName, emailOrUsername, StringComparison.OrdinalIgnoreCase))
                {
                    // Tìm thấy user có DisplayName = username -> trả về Email
                    return u.Email;
                }
            }

            // Không tìm thấy
            return null;
        }

        #region ====== ĐĂNG KÝ + LƯU USER VÀO REALTIME DATABASE ======

        /// <summary>
        /// Đăng ký tài khoản mới trên Firebase Auth
        /// đồng thời lưu thông tin User vào Realtime Database.
        /// </summary>
        /// <param name="user">Đối tượng user (thông tin hồ sơ: tên, email,...).</param>
        /// <param name="password">Mật khẩu đăng ký.</param>
        /// <exception cref="Exception">
        /// Nếu đăng ký thất bại hoặc không lấy được localId.
        /// </exception>
        public async Task RegisterAsync(User user, string password)
        {
            // Gửi yêu cầu đăng ký Firebase Auth
            var auth = await _http.PostAsync<dynamic>(SignUpUrl, new
            {
                email = user.Email,
                password = password,
                returnSecureToken = true
            }).ConfigureAwait(false);

            // Đăng ký thất bại
            if (auth == null || auth.error != null)
            {
                throw new Exception("Đăng ký thất bại.");
            }

            // Lấy localId Firebase sinh ra
            string localId = auth.localId;
            if (string.IsNullOrEmpty(localId))
            {
                throw new Exception("Không lấy được localId từ Firebase.");
            }

            // Lưu user vào Realtime Database: users/{localId}
            await _http.PutAsync(Db(string.Format("users/{0}", localId)), user)
                      .ConfigureAwait(false);

            // Lưu map email → true để check nhanh (key là email được base64 hóa)
            string enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));
            await _http.PutAsync(Db(string.Format("emails/{0}", enc)), true)
                      .ConfigureAwait(false);

            // Thêm trạng thái người dùng (offline = false hoặc tùy bạn đặt)
            await _http.PutAsync(Db($"status/{localId}"), new
            {
                Status = "offline"
            });
        }

        #endregion

        /// <summary>
        /// Đổi mật khẩu tài khoản Firebase.
        /// </summary>
        /// <param name="idToken">Token hiện tại (đã đăng nhập).</param>
        /// <param name="newPassword">Mật khẩu mới.</param>
        /// <returns>
        /// Tuple (success, newToken) – nếu thành công thì success = true
        /// và newToken là token mới Firebase trả về (nếu có).
        /// </returns>
        /// <exception cref="Exception">Nếu Firebase trả về lỗi.</exception>
        public async Task<(bool success, string newToken)> UpdatePasswordAsync(
            string idToken,
            string newPassword)
        {
            var res = await _http.PostAsync<dynamic>(UpdatePasswordUrl, new
            {
                idToken = idToken,
                password = newPassword,
                returnSecureToken = false
            }).ConfigureAwait(false);

            if (res == null || res.error != null)
            {
                // Firebase thường trả error.message, nhưng để an toàn ta check null trước
                string message = (res != null && res.error != null && res.error.message != null)
                    ? res.error.message
                    : "Firebase password update failed.";
                throw new Exception(message);
            }

            string newToken = res.idToken; // Firebase có thể trả token mới
            return (true, newToken);
        }

        #endregion

        #region ====== USERS (Realtime DB) ======

        /// <summary>
        /// Kiểm tra email đã tồn tại trong hệ thống hay chưa.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <returns>
        /// true nếu tồn tại; false nếu không.
        /// </returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            string enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            var res = await _http.GetAsync<object>(Db(string.Format("emails/{0}", enc)))
                                 .ConfigureAwait(false);

            return res != null;
        }

        /// <summary>
        /// Lấy avatar (chuỗi base64 hoặc URL) của người dùng theo localId.
        /// </summary>
        /// <param name="localId">Mã định danh người dùng.</param>
        /// <returns>Chuỗi avatar (base64/URL) hoặc null nếu chưa có.</returns>
        public async Task<string> GetAvatarAsync(string localId)
        {
            string key = KeySanitizer.SafeKey(localId);
            return await _http.GetAsync<string>(Db(string.Format("users/{0}/avatar", key)))
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// Cập nhật avatar của người dùng trong Realtime Database.
        /// </summary>
        /// <param name="localId">Mã định danh người dùng.</param>
        /// <param name="avatarBase64">Dữ liệu avatar dạng base64 (hoặc link, tùy DB của bạn).</param>
        public async Task UpdateAvatarAsync(string localId, string avatarBase64)
        {
            string key = KeySanitizer.SafeKey(localId);

            await _http.PatchAsync(Db(string.Format("users/{0}", key)), new
            {
                avatar = avatarBase64 // đúng field trong DB
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Cập nhật tên hiển thị (display name) của tài khoản.
        /// </summary>
        /// <param name="localId">Mã định danh người dùng.</param>
        /// <param name="newUsername">Tên hiển thị mới.</param>
        public async Task UpdateUsernameAsync(string localId, string newUsername)
        {
            string key = KeySanitizer.SafeKey(localId);

            await _http.PatchAsync(Db(string.Format("users/{0}", key)), new
            {
                displayName = newUsername // đúng field của user
            }).ConfigureAwait(false);
        }

        #endregion

        #region ====== STATUS (ONLINE / OFFLINE / CUSTOM) ======

        /// <summary>
        /// Cập nhật trạng thái người dùng (online/offline hoặc text tùy bạn).
        /// </summary>
        /// <param name="localId">Mã định danh người dùng.</param>
        /// <param name="status">Giá trị trạng thái (ví dụ: "online", "offline").</param>
        public async Task UpdateStatusAsync(string localId, string status)
        {
            string key = KeySanitizer.SafeKey(localId);

            // Lưu ý: path "status" phải trùng với nơi bạn đã lưu trong RegisterAsync
            await _http.PatchAsync(Db(string.Format("status/{0}", key)), new
            {
                Status = status
            }).ConfigureAwait(false);
        }

        #endregion

        #region ====== PASSWORD RESET (SEND EMAIL) ======

        /// <summary>
        /// Gửi email reset password thông qua Firebase Auth.
        /// </summary>
        /// <param name="email">Email cần gửi reset password.</param>
        /// <returns>
        /// true nếu gửi thành công, false nếu Firebase báo lỗi.
        /// </returns>
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var res = await _http.PostAsync<dynamic>(ResetPasswordUrl, new
            {
                requestType = "PASSWORD_RESET",
                email = email
            }).ConfigureAwait(false);

            if (res == null || res.error != null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ====== TẢI DỮ LIỆU USER (PROFILE) ======

        /// <summary>
        /// Tải thông tin chi tiết (hồ sơ) của một người dùng từ Realtime Database.
        /// Dùng để lấy FullName, DisplayName, Avatar... của người dùng khác.
        /// </summary>
        /// <param name="localId">ID của người dùng cần truy vấn (UID).</param>
        /// <returns>Đối tượng User chứa hồ sơ người dùng.</returns>
        public async Task<User> GetUserByIdAsync(string localId)
        {
            if (string.IsNullOrWhiteSpace(localId))
            {
                return null;
            }

            string safeId = KeySanitizer.SafeKey(localId);

            // Đường dẫn truy vấn: /users/{localId}
            string url = Db($"users/{safeId}");

            // Sử dụng HttpService để GET dữ liệu và deserialize sang User Model
            var userProfile = await _http.GetAsync<User>(url);

            if (userProfile != null)
            {
                // Gán LocalId vào User object để đảm bảo thông tin đầy đủ
                // (Đây là giải pháp cho vấn đề ta đã thảo luận trước đó)
                userProfile.LocalId = localId;
            }

            return userProfile;
        }

        #endregion
    }
}
