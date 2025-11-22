namespace ChatApp.Models.Users
{
    #region User Model
    /// <summary>
    /// Đại diện cho một tài khoản người dùng trong hệ thống ChatApp.
    /// Bao gồm thông tin đăng nhập, thông tin cá nhân và email liên hệ.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Tên tài khoản (username) dùng để đăng nhập.
        /// Đây cũng là key chính để lưu user trong Firebase.
        /// </summary>
        public string TaiKhoan { get; set; }

        /// <summary>
        /// Mật khẩu dạng plain-text (dùng Firebase Realtime DB nên chưa hash).
        /// </summary>
        public string MatKhau { get; set; }

        /// <summary>
        /// Email dùng để lấy lại mật khẩu và xác thực OTP.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Tên hiển thị của người dùng.
        /// </summary>
        public string Ten { get; set; }

        /// <summary>
        /// Ngày sinh (lưu dạng chuỗi dd/MM/yyyy).
        /// </summary>
        public string Ngaysinh { get; set; }

        /// <summary>
        /// Giới tính (Nam, Nữ, Khác...).
        /// </summary>
        public string Gioitinh { get; set; }
    }
    #endregion
}
