namespace ChatApp.Models.Users
{
    /// <summary>
    /// Mô tả thông tin hồ sơ người dùng trong ChatApp.
    /// Được lưu trên Firebase Realtime Database tại node "users/{localId}".
    /// </summary>
    public class User
    {
        #region ====== THUỘC TÍNH ======

        /// <summary>
        /// Tên hiển thị trong ứng dụng (nickname / username).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Họ tên đầy đủ của người dùng.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Địa chỉ email dùng để đăng nhập và nhận thông báo.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Ngày sinh, được lưu dạng chuỗi (ví dụ: "yyyy-MM-dd").
        /// </summary>
        public string Birthday { get; set; }

        /// <summary>
        /// Giới tính của người dùng (Nam/Nữ/Khác...).
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Dữ liệu avatar:
        /// - Có thể là chuỗi base64, hoặc URL tùy cách bạn thiết kế DB.
        /// </summary>
        public string Avatar { get; set; }

        #endregion
    }
}
