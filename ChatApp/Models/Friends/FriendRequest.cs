using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Models.Users;

namespace ChatApp.Models.Friends
{
    /// <summary>
    /// Biểu diễn một lời mời kết bạn (Friend Request) trên Firebase.
    /// </summary>
    public class FriendRequest
    {
        // Tên trường trong DB phải khớp: status
        public string status { get; set; }

        // Tên trường trong DB phải khớp: createdAt
        public long createdAt { get; set; } // Timestamp khi tạo request

        // --- THÔNG TIN BỔ SUNG CHO UI (Không lưu trong DB) ---
        // ID của người gửi (SenderId) hoặc người nhận (ReceiverId), tùy ngữ cảnh
        public string OtherUserId { get; set; }

        // Thông tin hồ sơ của người dùng liên quan
        public User Profile { get; set; }
    }
}