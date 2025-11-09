using System;
using System.Threading.Tasks;

using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    public class ChangePasswordController
    {
        private readonly AuthService _authService = new AuthService();

        // Đổi mật khẩu cho tài khoản
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
    }
}
