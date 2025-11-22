using System;
using System.Threading.Tasks;
using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý logic đổi mật khẩu:
    /// - Gọi <see cref="AuthService"/> để cập nhật mật khẩu mới cho tài khoản.
    /// - Trả về <c>true</c> nếu đổi thành công, <c>false</c> nếu có lỗi.
    /// </summary>
    public class ChangePasswordController
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Service xác thực dùng để cập nhật mật khẩu trên Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ======== Đổi mật khẩu ========

        /// <summary>
        /// Thực hiện đổi mật khẩu cho một tài khoản:
        /// - Gọi <see cref="AuthService.UpdatePasswordAsync(string, string)"/>.
        /// - Bao bọc try/catch để không làm crash UI.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần đổi mật khẩu.</param>
        /// <param name="matKhauMoi">Mật khẩu mới.</param>
        /// <returns>
        /// <c>true</c> nếu đổi mật khẩu thành công,
        /// <c>false</c> nếu xảy ra lỗi (network, Firebase, v.v.).
        /// </returns>
        public async Task<bool> DoiMatKhauAsync(string taiKhoan, string matKhauMoi)
        {
            try
            {
                await _authService.UpdatePasswordAsync(taiKhoan, matKhauMoi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
