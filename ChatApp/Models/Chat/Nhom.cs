using System;
using System.Collections.Generic;

namespace ChatApp.Models.Chat
{
    public class GroupMemberInfo
    {
        // Có phải quản trị viên không
        public bool IsAdmin { get; set; }

        // Cấp độ hiển thị:
        // "gold"  = nút vàng
        // "silver"= nút bạc
        // "member" hoặc null = thường
        public string Tier { get; set; } = "member";

        // Thời điểm bị mute tới (Unix time UTC).
        // 0 hoặc âm = không bị cấm chat
        public long MutedUntil { get; set; } = 0;
    }

    public class Nhom
    {
        public string id { get; set; }
        public string tenNhom { get; set; }
        public string moTa { get; set; }
        public string taoBoi { get; set; }
        public long createdAt { get; set; }

        // Chế độ chỉ cho admin vàng/bạc nhắn tin
        public bool AdminOnlyChat { get; set; } = false;

        // Bật phê duyệt thành viên mới (chưa implement logic join, chỉ lưu cờ)
        public bool RequireApproval { get; set; } = false;

        // key = tên user, value = info (quyền + tier + mute)
        public Dictionary<string, GroupMemberInfo> thanhVien { get; set; }
            = new Dictionary<string, GroupMemberInfo>(StringComparer.OrdinalIgnoreCase);
    }
}
