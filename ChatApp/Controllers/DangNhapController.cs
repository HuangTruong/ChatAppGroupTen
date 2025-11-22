using System;
using System.Threading.Tasks;
using ChatApp.Models.Users;
using ChatApp.Services.Auth;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý logic đăng nhập:
    /// - Kiểm tra input tài khoản/mật khẩu từ UI.
    /// - Lấy thông tin người dùng từ Firebase bằng <see cref="AuthService"/>.
    /// - So khớp mật khẩu và báo lỗi nếu không hợp lệ.
    /// - Cập nhật trạng thái ONLINE khi đăng nhập thành công.
    /// </summary>
    public class LoginController
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Service xác thực, dùng để thao tác với dữ liệu người dùng trên Firebase.
        /// </summary>
        private readonly AuthService _authService;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo <see cref="LoginController"/>:
        /// - Tạo Firebase client qua <see cref="FirebaseClientFactory.Create"/>.
        /// - Tạo instance <see cref="AuthService"/> dùng chung cho toàn bộ quá trình đăng nhập.
        /// </summary>
        public LoginController()
        {
            var client = FirebaseClientFactory.Create();
            _authService = new AuthService(client);
        }

        #endregion

        #region ======== Đăng nhập ========

        /// <summary>
        /// Thực hiện đăng nhập với tài khoản và mật khẩu:
        /// 1. Kiểm tra rỗng cho tên đăng nhập và mật khẩu.
        /// 2. Lấy user từ Firebase theo <paramref name="taiKhoan"/>.
        /// 3. Nếu không tìm thấy user -> báo lỗi "Tài khoản không tồn tại!".
        /// 4. Nếu mật khẩu không khớp -> báo lỗi "Mật khẩu không đúng!".
        /// 5. Nếu thành công -> cập nhật trạng thái ONLINE và trả về đối tượng <see cref="User"/>.
        /// </summary>
        /// <param name="taiKhoan">Tên đăng nhập người dùng.</param>
        /// <param name="matKhau">Mật khẩu người dùng.</param>
        /// <returns>
        /// Đối tượng <see cref="User"/> tương ứng nếu đăng nhập thành công.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Ném ra nếu tên đăng nhập hoặc mật khẩu bị bỏ trống.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Ném ra nếu tài khoản không tồn tại hoặc mật khẩu không chính xác.
        /// </exception>
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

            // Đăng nhập thành công -> cập nhật trạng thái ONLINE
            await _authService.UpdateStatusAsync(user.Ten, "online");

            return user;
        }

        #endregion
    }
}
