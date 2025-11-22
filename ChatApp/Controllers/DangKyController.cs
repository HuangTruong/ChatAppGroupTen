using ChatApp.Models.Users;
using ChatApp.Services;
using ChatApp.Services.Auth;
using System;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý logic đăng ký tài khoản mới:
    /// - Kiểm tra hợp lệ thông tin đăng ký (đủ trường, trùng mật khẩu, trùng tài khoản, trùng email).
    /// - Gọi <see cref="AuthService"/> để thao tác với Firebase.
    /// - Hỗ trợ kiểm tra tồn tại email / tài khoản trước khi gửi OTP.
    /// </summary>
    public class DangKyController
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Service xác thực, dùng để kiểm tra và đăng ký tài khoản trên Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ======== Đăng ký tài khoản chính ========

        /// <summary>
        /// Thực hiện đăng ký tài khoản mới:
        /// 1. Kiểm tra người dùng đã nhập đủ các trường bắt buộc (tài khoản, mật khẩu, xác nhận mật khẩu, email, tên, ngày sinh, giới tính).
        /// 2. Kiểm tra mật khẩu và xác nhận mật khẩu trùng nhau.
        /// 3. Kiểm tra tài khoản đã tồn tại hay chưa.
        /// 4. Kiểm tra email đã được dùng cho tài khoản khác hay chưa.
        /// 5. Nếu tất cả hợp lệ thì gọi <see cref="AuthService.RegisterAsync(User)"/> để đăng ký lên Firebase.
        /// </summary>
        /// <param name="user">Đối tượng <see cref="User"/> chứa thông tin đăng ký.</param>
        /// <param name="xacNhanMatKhau">Chuỗi xác nhận mật khẩu người dùng nhập lại.</param>
        /// <exception cref="Exception">
        /// Ném ra khi:
        /// - Thiếu thông tin bắt buộc.
        /// - Mật khẩu và xác nhận mật khẩu không khớp.
        /// - Tên tài khoản đã tồn tại.
        /// - Email đã tồn tại.
        /// </exception>
        public async Task DangKyAsync(User user, string xacNhanMatKhau)
        {
            // 1. Kiểm tra đủ thông tin
            if (string.IsNullOrWhiteSpace(user.TaiKhoan) ||
                string.IsNullOrWhiteSpace(user.MatKhau) ||
                string.IsNullOrWhiteSpace(xacNhanMatKhau) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Ten) ||
                string.IsNullOrWhiteSpace(user.Ngaysinh) ||
                string.IsNullOrWhiteSpace(user.Gioitinh))
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin!");
            }

            // 2. Xác nhận mật khẩu
            if (user.MatKhau != xacNhanMatKhau)
                throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp!");

            // 3. Trùng tài khoản
            if (await _authService.GetUserAsync(user.TaiKhoan) != null)
                throw new Exception("Tên tài khoản đã tồn tại!");

            // 4. Trùng email
            if (await _authService.EmailExistsAsync(user.Email))
                throw new Exception("Email đã tồn tại!");

            // 6. Đăng ký lên Firebase
            await _authService.RegisterAsync(user);
        }

        #endregion

        #region ======== Kiểm tra tồn tại Email / Tài khoản (dùng trước khi gửi OTP) ========

        /// <summary>
        /// Kiểm tra email đã tồn tại trong hệ thống hay chưa:
        /// - Trả về <c>false</c> nếu email rỗng hoặc null.
        /// - Ngược lại gọi <see cref="AuthService.EmailExistsAsync(string)"/>.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <returns>
        /// <c>true</c> nếu email đã tồn tại trong hệ thống,
        /// <c>false</c> nếu chưa tồn tại hoặc chuỗi email rỗng/null.
        /// </returns>
        public async Task<bool> KiemTraEmailTonTaiAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();
            return await _authService.EmailExistsAsync(email);
        }

        /// <summary>
        /// Kiểm tra tài khoản (username) đã tồn tại trong hệ thống hay chưa:
        /// - Trả về <c>false</c> nếu tài khoản rỗng hoặc null.
        /// - Ngược lại gọi <see cref="AuthService.GetUserAsync(string)"/> và kiểm tra kết quả khác null.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần kiểm tra.</param>
        /// <returns>
        /// <c>true</c> nếu tài khoản đã tồn tại,
        /// <c>false</c> nếu chưa tồn tại hoặc chuỗi tài khoản rỗng/null.
        /// </returns>
        public async Task<bool> KiemTraTaiKhoanTonTaiAsync(string taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan))
                return false;

            taiKhoan = taiKhoan.Trim();
            var user = await _authService.GetUserAsync(taiKhoan);
            return user != null;
        }

        #endregion
    }
}
