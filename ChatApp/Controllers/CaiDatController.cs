using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller cho màn hình Cài đặt:
    /// - Tải avatar hiện tại.
    /// - Cập nhật avatar.
    /// - Đổi tên hiển thị.
    /// - Đổi mật khẩu (và cập nhật lại token).
    /// </summary>
    public class CaiDatController
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase.
        /// </summary>
        private readonly AuthService _authService;

        /// <summary>
        /// Mã người dùng Firebase (localId).
        /// </summary>
        private readonly string _localId;

        /// <summary>
        /// Token hiện tại của người dùng. Có thể được cập nhật khi đổi mật khẩu.
        /// </summary>
        private string _token; // bỏ readonly để cập nhật token mới

        #endregion

        #region ====== KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo controller cài đặt với localId và token hiện tại.
        /// </summary>
        public CaiDatController(string localId, string token)
        {
            _localId = localId;
            _token = token;
            _authService = new AuthService();
        }

        #endregion

        #region ====== LẤY AVATAR ======

        /// <summary>
        /// Tải avatar người dùng từ Firebase (base64 → Image).
        /// </summary>
        /// <returns>
        /// Đối tượng <see cref="Image"/> nếu có avatar, 
        /// hoặc null nếu không có / bị lỗi.
        /// </returns>
        public async Task<Image> LoadAvatarAsync()
        {
            try
            {
                string base64 = await _authService.GetAvatarAsync(_localId);
                if (string.IsNullOrEmpty(base64))
                {
                    return null;
                }

                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                // Có thể log ra nếu cần, hiện tại trả null cho UI tự xử lý
                return null;
            }
        }

        #endregion

        #region ====== CẬP NHẬT AVATAR ======

        /// <summary>
        /// Cập nhật avatar người dùng từ file ảnh trên máy.
        /// </summary>
        /// <param name="filePath">Đường dẫn file ảnh người dùng chọn.</param>
        /// <returns>true nếu cập nhật thành công, false nếu lỗi.</returns>
        public async Task<bool> UpdateAvatarAsync(string filePath)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(bytes);

                await _authService.UpdateAvatarAsync(_localId, base64);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật avatar: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region ====== ĐỔI TÊN HIỂN THỊ ======

        /// <summary>
        /// Đổi tên hiển thị (display name) của người dùng.
        /// </summary>
        /// <param name="newUsername">Tên hiển thị mới.</param>
        /// <returns>true nếu cập nhật thành công, false nếu lỗi.</returns>
        public async Task<bool> ChangeUsernameAsync(string newUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newUsername))
                {
                    MessageBox.Show("Tên hiển thị không được trống!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                await _authService.UpdateUsernameAsync(_localId, newUsername);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi tên hiển thị: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region ====== ĐỔI MẬT KHẨU ======

        /// <summary>
        /// Đổi mật khẩu tài khoản hiện tại, đồng thời cập nhật token mới.
        /// </summary>
        /// <param name="newPassword">Mật khẩu mới.</param>
        /// <returns>true nếu đổi thành công, false nếu lỗi.</returns>
        public async Task<bool> ChangePasswordAsync(string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Mật khẩu mới không được để trống!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var result = await _authService.UpdatePasswordAsync(_token, newPassword);

                if (!result.success)
                {
                    MessageBox.Show("Đổi mật khẩu thất bại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Cập nhật token mới sau khi đổi mật khẩu
                _token = result.newToken;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi mật khẩu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
    }
}
