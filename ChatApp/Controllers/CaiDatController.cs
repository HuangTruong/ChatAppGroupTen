using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Services.Firebase;
using Newtonsoft.Json.Linq;

namespace ChatApp.Controllers
{
    public class CaiDatController
    {
        #region ====== DTO ======

        public class UserProfileVm
        {
            public string Email { get; set; }
            public string UserName { get; set; }
            public string DisplayName { get; set; }
            public string Gender { get; set; }
            public string Birthday { get; set; }
        }

        #endregion

        #region ====== FIELDS ======

        private readonly AuthService _authService;
        private readonly string _localId;
        private string _token;

        #endregion

        #region ====== CTOR ======

        public CaiDatController(string localId, string token)
        {
            _localId = localId;
            _token = token;
            _authService = new AuthService();
        }

        #endregion

        #region ====== LOAD PROFILE ======

        private static string GetStr(JObject obj, params string[] keys)
        {
            if (obj == null || keys == null) return null;

            for (int i = 0; i < keys.Length; i++)
            {
                string k = keys[i];
                if (string.IsNullOrWhiteSpace(k)) continue;

                JToken t = obj[k];
                if (t != null && t.Type != JTokenType.Null)
                {
                    string s = t.ToString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            }

            return null;
        }

        public async Task<UserProfileVm> LoadProfileAsync()
        {
            try
            {
                JObject raw = await _authService.GetUserProfileRawAsync(_localId).ConfigureAwait(false);
                if (raw == null) return new UserProfileVm();

                UserProfileVm vm = new UserProfileVm();
                vm.Email = GetStr(raw,"Email");
                vm.UserName = GetStr(raw, "UserName");
                vm.DisplayName = GetStr(raw, "DisplayName");
                vm.Gender = GetStr(raw,"Gender");
                vm.Birthday = GetStr(raw, "Birthday");

                return vm;
            }
            catch
            {
                return new UserProfileVm();
            }
        }

        #endregion

        #region ====== AVATAR ======

        public async Task<Image> LoadAvatarAsync()
        {
            try
            {
                string base64 = await _authService.GetAvatarAsync(_localId).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(base64) ||
                    string.Equals(base64, "null", StringComparison.OrdinalIgnoreCase))
                    return null;

                byte[] bytes = Convert.FromBase64String(base64);

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image img = Image.FromStream(ms);
                    return (Image)img.Clone();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateAvatarAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                byte[] bytes = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(bytes);

                await _authService.UpdateAvatarAsync(_localId, base64).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật avatar: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region ====== UPDATE PROFILE (TRỪ EMAIL) ======

        public async Task<bool> ChangeUserNameAsync(string newUserName)
        {
            try
            {
                newUserName = (newUserName ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(newUserName))
                {
                    MessageBox.Show("Tên đăng nhập không được trống!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (newUserName.Contains("@"))
                {
                    MessageBox.Show("Tên đăng nhập không nên chứa ký tự '@'.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (newUserName.Contains(" "))
                {
                    MessageBox.Show("Tên đăng nhập không được chứa khoảng trắng.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                bool exists = await _authService.UserNameExistsAsync(newUserName, _localId).ConfigureAwait(false);
                if (exists)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại, hãy chọn tên khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                await _authService.UpdateUserNameAsync(_localId, newUserName).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi tên đăng nhập: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> ChangeDisplayNameAsync(string newDisplayName)
        {
            try
            {
                newDisplayName = (newDisplayName ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(newDisplayName))
                {
                    MessageBox.Show("Tên hiển thị không được trống!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // UpdateUsernameAsync đang patch displayName
                await _authService.UpdateDisplayNameAsync(_localId, newDisplayName).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi tên hiển thị: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> ChangeGenderAsync(string gender)
        {
            try
            {
                gender = (gender ?? string.Empty).Trim();
                await _authService.UpdateGenderAsync(_localId, gender).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi giới tính: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> ChangeBirthdayAsync(string birthdayText)
        {
            try
            {
                birthdayText = (birthdayText ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(birthdayText))
                {
                    await _authService.UpdateBirthdayAsync(_localId, string.Empty).ConfigureAwait(false);
                    return true;
                }

                DateTime dt;
                string[] fmts = new string[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "M/d/yyyy" };

                bool ok = DateTime.TryParseExact(
                    birthdayText,
                    fmts,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt);

                if (!ok)
                {
                    ok = DateTime.TryParse(birthdayText, out dt);
                }

                if (!ok)
                {
                    MessageBox.Show("Ngày sinh không hợp lệ. Ví dụ: 31/12/2005", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string store = dt.ToString("dd/MM/yyyy");
                await _authService.UpdateBirthdayAsync(_localId, store).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi ngày sinh: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Giữ tương thích code cũ
        public Task<bool> ChangeUsernameAsync(string newDisplayName)
        {
            return ChangeDisplayNameAsync(newDisplayName);
        }

        #endregion

        #region ====== PASSWORD ======

        public async Task<bool> ChangePasswordAsync(string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Mật khẩu mới không được để trống!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var result = await _authService.UpdatePasswordAsync(_token, newPassword).ConfigureAwait(false);

                if (!result.success)
                {
                    MessageBox.Show("Đổi mật khẩu thất bại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                _token = result.newToken;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi mật khẩu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
    }
}
