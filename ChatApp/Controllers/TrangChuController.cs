using System.Threading.Tasks;

using ChatApp.Models.Users;
using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    public class TrangChuController
    {
        private readonly AuthService _authService = new AuthService();

        // Lấy thông tin người dùng từ tầng Service (Firebase)
        public async Task<User> GetUserAsync(string taiKhoan)
        {
            return await _authService.GetUserAsync(taiKhoan);
        }

        // Cập nhật trạng thái online/offline
        public async Task CapNhatTrangThaiAsync(string taiKhoan, string trangThai)
        {
            await _authService.UpdateStatusAsync(taiKhoan, trangThai);
        }
    }
}
