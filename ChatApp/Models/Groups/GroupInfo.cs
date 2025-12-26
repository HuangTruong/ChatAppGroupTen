using System;

namespace ChatApp.Models.Groups
{
    /// <summary>
    /// Thông tin nhóm chat (metadata).
    /// Lưu ở node: groups/{groupId}.
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// ID nhóm (key trên Firebase).
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Tên nhóm hiển thị.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Người tạo nhóm (safe localId).
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Thời điểm tạo nhóm (Unix ms).
        /// </summary>
        public long CreatedAt { get; set; }

        /// <summary>
        /// Số lượng thành viên (để hiển thị nhanh).
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Tin nhắn cuối cùng (preview).
        /// </summary>
        public string LastMessage { get; set; }

        /// <summary>
        /// Thời điểm tin nhắn cuối (Unix ms).
        /// </summary>
        public long LastMessageAt { get; set; }

        public string Avatar { get; set; }
    }
}
