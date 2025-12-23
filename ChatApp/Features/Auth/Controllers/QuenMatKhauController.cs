using System;
using System.Threading.Tasks;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý luồng "Quên mật khẩu":
    /// - Nhận email từ UI.
    /// - Gọi AuthService để gửi email reset password qua Firebase.
    /// </summary>
    public class ForgotPasswordController
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase Authentication.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ====== PUBLIC METHODS ======

        /// <summary>
        /// Xử lý yêu cầu quên mật khẩu:
        /// - Kiểm tra email hợp lệ.
        /// - Gọi Firebase gửi link reset password đến email đó.
        /// </summary>
        /// <param name="email">Email đã đăng ký tài khoản.</param>
        /// <returns>
        /// true nếu Firebase chấp nhận gửi email reset;
        /// false nếu email trống hoặc có lỗi khi gọi service.
        /// </returns>
        public async Task<bool> QuenMatKhauAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // Gọi Firebase để gửi link reset
            bool success = await _authService.SendPasswordResetEmailAsync(email);
            return success;
        }

        #endregion
    }
}
