using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý luồng Đăng ký:
    /// - Validate dữ liệu form đăng ký.
    /// - Kiểm tra trùng email, định dạng email.
    /// - Gọi AuthService để đăng ký tài khoản mới lên Firebase.
    /// </summary>
    public class DangKyController
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase (Auth + Realtime Database).
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ====== ĐĂNG KÝ TÀI KHOẢN ======

        /// <summary>
        /// Logic đăng ký chính:
        /// - Kiểm tra dữ liệu bắt buộc.
        /// - Kiểm tra mật khẩu & xác nhận mật khẩu.
        /// - Kiểm tra email trùng và định dạng email.
        /// - Gọi Firebase để tạo tài khoản và lưu user.
        /// </summary>
        /// <param name="user">Đối tượng người dùng (thông tin hồ sơ).</param>
        /// <param name="password">Mật khẩu.</param>
        /// <param name="confirmpassword">Xác nhận mật khẩu.</param>
        /// <exception cref="Exception">
        /// Ném ra với message tiếng Việt để hiển thị trực tiếp lên UI khi:
        /// - Thiếu thông tin.
        /// - Mật khẩu không khớp.
        /// - Email đã tồn tại.
        /// - Định dạng email không hợp lệ.
        /// - Lỗi khi gọi Firebase đăng ký.
        /// </exception>
        public async Task DangKyAsync(User user, string password, string confirmpassword)
        {
            // 1. Kiểm tra đủ thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(user.UserName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmpassword) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.DisplayName) ||
                string.IsNullOrWhiteSpace(user.Birthday) ||
                string.IsNullOrWhiteSpace(user.Gender))
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin!");
            }

            // 2. Xác nhận mật khẩu
            if (password != confirmpassword)
            {
                throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp!");
            }

            // 3. Kiểm tra định dạng email
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (string.IsNullOrWhiteSpace(user.Email) || !Regex.IsMatch(user.Email, pattern))
            {
                throw new Exception("Định dạng email không hợp lệ.");
            }

            // 4. Kiểm tra email đã tồn tại chưa
            if (await _authService.EmailExistsAsync(user.Email))
            {
                throw new Exception("Email đã tồn tại!");
            }

            // 5. Kiểm tra DisplayName không chứa ký tự đặc biệt
            // Chỉ cho phép: a-z, A-Z, 0-9, 
            const string displayNamePattern = @"^[a-zA-Z0-9]+$";
            if (!Regex.IsMatch(user.UserName, displayNamePattern))
            {
                throw new Exception("Tên hiển thị chỉ được chứa chữ, số (không có khoảng trắng hoặc ký tự đặc biệt khác).");
            }

            // 6. Đăng ký lên Firebase
            try
            {
                await _authService.RegisterAsync(user, password);
            }
            catch
            {
                // Ném lại exception gốc để UI xử lý và hiển thị message phù hợp
                throw;
            }
        }

        #endregion
    }
}
