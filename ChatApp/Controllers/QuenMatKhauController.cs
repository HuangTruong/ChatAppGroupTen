using System;
using System.Threading.Tasks;
using ChatApp.Models.Otp;
using ChatApp.Services.Auth;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý luồng quên mật khẩu:
    /// - Tạo / lưu mã OTP lên Firebase.
    /// - Tra cứu tài khoản từ email.
    /// - Kiểm tra OTP hợp lệ và dọn dẹp sau khi dùng.
    /// - Gửi OTP qua email cho người dùng.
    /// </summary>
    public class ForgotPasswordController
    {
        #region ======== Trường / Services ========

        /// <summary>
        /// Service xác thực dùng để tra cứu tài khoản / kiểm tra account-email.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        /// <summary>
        /// Service OTP dùng để lưu / lấy / xoá OTP và gửi email.
        /// </summary>
        private readonly OtpService _otpService = new OtpService();

        #endregion

        #region ======== TẠO OTP CHỈ TỪ EMAIL (FORM CHỈ NHẬP EMAIL) ========

        /// <summary>
        /// Tạo và lưu mã OTP mới chỉ từ email:
        /// 1. Tra cứu tài khoản tương ứng với email từ Firebase.
        /// 2. Nếu tìm thấy tài khoản thì tái sử dụng hàm
        ///    <see cref="TaoVaLuuOtpAsync(string, string)"/> để tạo OTP.
        /// 3. Trả về chuỗi mã OTP nếu thành công, hoặc <c>null</c> nếu không hợp lệ.
        /// </summary>
        /// <param name="email">Địa chỉ email người dùng nhập.</param>
        /// <returns>
        /// Mã OTP dạng chuỗi nếu tạo thành công,
        /// <c>null</c> nếu email rỗng hoặc không tìm thấy tài khoản tương ứng.
        /// </returns>
        public async Task<string> TaoVaLuuOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // 1) Tìm tài khoản trong Firebase theo email
            string taiKhoan = await _authService.GetAccountByEmailAsync(email);
            if (string.IsNullOrWhiteSpace(taiKhoan))
            {
                // Không có user nào trùng email
                return null;
            }

            // 2) Tái sử dụng hàm cũ: kiểm tra + tạo OTP + lưu vào Firebase
            return await TaoVaLuuOtpAsync(taiKhoan, email);
        }

        /// <summary>
        /// Tra cứu tài khoản (username) tương ứng với email:
        /// - Chỉ wrap lại <see cref="AuthService.GetAccountByEmailAsync(string)"/> để form gọi cho gọn.
        /// </summary>
        /// <param name="email">Địa chỉ email.</param>
        /// <returns>
        /// Tên tài khoản nếu tìm thấy, ngược lại <c>null</c>.
        /// </returns>
        public Task<string> TimTaiKhoanBangEmailAsync(string email)
        {
            // chỉ wrap lại cho gọn, sau này nếu đổi logic thì chỉnh 1 nơi
            return _authService.GetAccountByEmailAsync(email);
        }

        #endregion

        #region ======== TẠO / LƯU OTP (KHI ĐÃ BIẾT ACCOUNT + EMAIL) ========

        /// <summary>
        /// Tạo và lưu mã OTP mới khi đã biết sẵn tài khoản và email:
        /// 1. Kiểm tra cặp (tài khoản, email) có khớp hay không bằng
        ///    <see cref="AuthService.IsAccountEmailAsync(string, string)"/>.
        /// 2. Nếu hợp lệ:
        ///    - Tạo mã OTP ngẫu nhiên 6 chữ số (100000–999999).
        ///    - Thiết lập thời gian hết hạn sau 5 phút (UTC).
        ///    - Lưu vào Firebase qua <see cref="OtpService.SaveOtpAsync(string, ThongTinMaFirebase)"/>.
        /// 3. Trả về mã OTP vừa tạo, hoặc <c>null</c> nếu cặp account-email không hợp lệ.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản.</param>
        /// <param name="email">Email liên kết với tài khoản.</param>
        /// <returns>
        /// Mã OTP nếu tạo thành công, <c>null</c> nếu cặp account/email không hợp lệ.
        /// </returns>
        public async Task<string> TaoVaLuuOtpAsync(string taiKhoan, string email)
        {
            // Kiểm tra tài khoản + email có hợp lệ
            bool hopLe = await _authService.IsAccountEmailAsync(taiKhoan, email);
            if (!hopLe) return null;

            string maOtp = new Random().Next(100000, 999999).ToString();
            DateTime hetHan = DateTime.UtcNow.AddMinutes(5);

            var otpInfo = new ThongTinMaFirebase
            {
                Ma = maOtp,
                HetHanLuc = hetHan.ToString("o")
            };

            await _otpService.SaveOtpAsync(taiKhoan, otpInfo);

            return maOtp;
        }

        #endregion

        #region ======== KIỂM TRA OTP HỢP LỆ ========

        /// <summary>
        /// Kiểm tra mã OTP người dùng nhập có hợp lệ hay không:
        /// - Lấy OTP lưu trên Firebase bằng <see cref="OtpService.GetOtpAsync(string)"/>.
        /// - Parse thời gian hết hạn và so sánh với <see cref="DateTime.UtcNow"/>.
        /// - OTP hợp lệ nếu:
        ///   + Chưa hết hạn, và
        ///   + Mã nhập trùng với mã lưu.
        /// - Sau khi OTP được xác nhận thành công hoặc đã hết hạn, sẽ xóa luôn
        ///   bằng <see cref="OtpService.DeleteOtpAsync(string)"/>.
        /// </summary>
        /// <param name="taiKhoan">Tên tài khoản cần kiểm tra OTP.</param>
        /// <param name="maNhap">Mã OTP người dùng nhập.</param>
        /// <returns>
        /// <c>true</c> nếu OTP hợp lệ,
        /// <c>false</c> nếu OTP không tồn tại, sai hoặc đã hết hạn.
        /// </returns>
        public async Task<bool> KiemTraOtpHopLeAsync(string taiKhoan, string maNhap)
        {
            var otp = await _otpService.GetOtpAsync(taiKhoan);
            if (otp == null) return false;

            DateTime hetHan;
            if (!DateTime.TryParse(otp.HetHanLuc, out hetHan))
                return false;

            bool hopLe = DateTime.UtcNow <= hetHan && otp.Ma == maNhap;

            // Nếu OTP đã xác nhận thành công hoặc đã hết hạn thì xóa luôn
            if (hopLe || DateTime.UtcNow > hetHan)
            {
                await _otpService.DeleteOtpAsync(taiKhoan);
            }

            return hopLe;
        }

        #endregion

        #region ======== GỬI OTP QUA EMAIL ========

        /// <summary>
        /// Gửi mã OTP đến email người nhận:
        /// - Uỷ quyền cho <see cref="OtpService.GuiEmailOtp(string, string)"/>.
        /// - Không bắt exception ở đây, để caller tự xử lý nếu cần.
        /// </summary>
        /// <param name="emailNhan">Địa chỉ email nhận OTP.</param>
        /// <param name="maOtp">Mã OTP sẽ gửi.</param>
        public void GuiEmailOtp(string emailNhan, string maOtp)
        {
            _otpService.GuiEmailOtp(emailNhan, maOtp);
        }

        #endregion
    }
}
