using System;
using System.Threading.Tasks;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý luồng đăng nhập:
    /// - Validate input từ UI.
    /// - Gọi AuthService để đăng nhập Firebase.
    /// - Cập nhật trạng thái người dùng sang "online".
    /// </summary>
    public class LoginController
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase Authentication và Realtime Database.
        /// </summary>
        private readonly AuthService _authService;

        #endregion

        #region ====== KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo LoginController với AuthService mặc định.
        /// </summary>
        public LoginController()
        {
            _authService = new AuthService();
        }

        #endregion

        #region ====== ĐĂNG NHẬP ======

        /// <summary>
        /// Xử lý đăng nhập:
        /// - Kiểm tra email / mật khẩu không được để trống.
        /// - Gọi Firebase Auth để đăng nhập.
        /// - Nếu thành công, cập nhật trạng thái "online" cho user.
        /// </summary>
        /// <param name="email">Email đăng nhập.</param>
        /// <param name="password">Mật khẩu đăng nhập.</param>
        /// <returns>
        /// Tuple (localId, token) do Firebase trả về.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Nếu email hoặc mật khẩu bị bỏ trống.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Nếu Firebase không trả về localId hợp lệ.
        /// </exception>
        public async Task<(string localId, string token)> DangNhapAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Vui lòng nhập email!");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Vui lòng nhập mật khẩu!");
            }

            // Đăng nhập qua Firebase Auth
            var result = await _authService.LoginAsync(email, password);
            string localId = result.localId;
            string token = result.token;

            if (string.IsNullOrEmpty(localId))
            {
                throw new InvalidOperationException("Tài khoản không tồn tại hoặc mật khẩu không đúng!");
            }

            // Cập nhật trạng thái ONLINE
            await _authService.UpdateStatusAsync(localId, "online");

            return (localId, token);
        }

        #endregion
    }
}
