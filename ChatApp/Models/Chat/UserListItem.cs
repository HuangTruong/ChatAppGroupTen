using System;

namespace ChatApp.Models.Chat
{
    #region UserListItem Model
    /// <summary>
    /// Mục hiển thị một người dùng trong danh sách bên trái
    /// (danh sách bạn bè, người lạ, yêu cầu kết bạn).
    /// </summary>
    public class UserListItem
    {
        /// <summary>
        /// Tên hiển thị của người dùng (username).
        /// Đây là khóa để truy vấn Firebase.
        /// </summary>
        public string TenHienThi { get; set; }

        /// <summary>
        /// True nếu người dùng này đã là bạn bè với tài khoản hiện tại.
        /// </summary>
        public bool LaBanBe { get; set; }

        /// <summary>
        /// True nếu mình đã gửi lời mời kết bạn cho người này,
        /// nhưng họ chưa chấp nhận.
        /// </summary>
        public bool DaGuiLoiMoi { get; set; }

        /// <summary>
        /// True nếu người này đã gửi lời mời kết bạn cho mình.
        /// </summary>
        public bool MoiKetBanChoMinh { get; set; }

        /// <summary>
        /// Trạng thái online/offline (theo Firebase node /status).
        /// </summary>
        public bool Online { get; set; }
    }
    #endregion
}
