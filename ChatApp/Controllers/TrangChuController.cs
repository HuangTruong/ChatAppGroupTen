using System.Threading.Tasks;
using ChatApp.Models.Users;
using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller hỗ trợ cho màn hình Trang chủ:
    /// - Lấy thông tin người dùng từ Firebase.
    /// - Cập nhật trạng thái online/offline cho người dùng.
    /// </summary>
    public class TrangChuController
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Service xác thực dùng để thao tác với dữ liệu user (Firebase).
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ======== Lấy thông tin người dùng ========

        /// <summary>
        /// Lấy thông tin người dùng từ tầng Service (Firebase) theo tài khoản.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần truy vấn.</param>
        /// <returns>
        /// Đối tượng <see cref="User"/> nếu tìm thấy,
        /// hoặc <c>null</c> nếu không tồn tại.
        /// </returns>
        public async Task<User> GetUserAsync(string taiKhoan)
        {
            return await _authService.GetUserAsync(taiKhoan);
        }

        #endregion

        #region ======== Cập nhật trạng thái online/offline ========

        /// <summary>
        /// Cập nhật trạng thái online/offline cho người dùng:
        /// - Gọi <see cref="AuthService.UpdateStatusAsync(string, string)"/>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần cập nhật.</param>
        /// <param name="trangThai">Trạng thái mới (vd: "online", "offline").</param>
        public async Task CapNhatTrangThaiAsync(string taiKhoan, string trangThai)
        {
            await _authService.UpdateStatusAsync(taiKhoan, trangThai);
        }

        #endregion
    }
}
