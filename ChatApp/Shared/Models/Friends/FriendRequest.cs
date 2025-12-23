using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Models.Users;

namespace ChatApp.Models.Friends
{
    /// <summary>
    /// Biểu diễn một lời mời kết bạn (Friend Request) được lưu trữ trên Firebase Realtime Database.
    /// Đối tượng này được sử dụng để ghi đồng thời tại hai vị trí độc lập nhằm mục đích theo dõi trạng thái lời mời.
    /// Dựa trên cấu trúc DB:
    /// 1. Lời mời ĐÃ GỬI (Outgoing Request) được lưu tại: /outgoingRequests/{SenderId}/{ReceiverId}
    /// 2. Lời mời ĐÃ NHẬN (Incoming Request) được lưu tại: /friendRequests/{ReceiverId}/{SenderId}
    /// </summary>
    public class FriendRequest
    {
        #region ====== THUỘC TÍNH CƠ BẢN (LƯU TRONG DB) ======

        /// <summary>
        /// Trạng thái của lời mời kết bạn (Ví dụ: pending, accepted, rejected).
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Dấu thời gian (timestamp - epoch milliseconds) khi lời mời được tạo.
        /// </summary>
        public long createdAt { get; set; }

        /// <summary>
        /// ID của người dùng liên quan đến lời mời (Người gửi hoặc Người nhận), 
        /// tùy thuộc vào ngữ cảnh đang xem (Yêu cầu đã gửi hay đã nhận).
        /// Dùng để dễ dàng tra cứu hồ sơ người dùng đó.
        /// </summary>
        public string OtherUserId { get; set; }

        /// <summary>
        /// Thông tin hồ sơ đầy đủ của người dùng liên quan đến lời mời.
        /// </summary>
        public User Profile { get; set; }

        #endregion
    }
}