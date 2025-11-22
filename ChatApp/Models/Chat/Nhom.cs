using System;
using System.Collections.Generic;

namespace ChatApp.Models.Chat
{
    #region GroupMemberInfo
    /// <summary>
    /// Thông tin của một thành viên trong nhóm:
    /// quyền quản trị, tier hiển thị và trạng thái mute.
    /// </summary>
    public class GroupMemberInfo
    {
        /// <summary>
        /// Cho biết thành viên có phải quản trị viên (admin) hay không.
        /// Dùng cho các nhóm có nhiều mức admin khác nhau.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Cấp độ thành viên:
        /// - "gold"  : quản trị cấp cao nhất (nút vàng)
        /// - "silver": quản trị viên (nút bạc)
        /// - "member": thành viên thường
        /// Mặc định là "member".
        /// </summary>
        public string Tier { get; set; } = "member";

        /// <summary>
        /// Thời điểm bị mute tính bằng UnixTime (UTC).
        /// Nếu giá trị &lt;= 0 nghĩa là không bị mute.
        /// </summary>
        public long MutedUntil { get; set; } = 0;
    }
    #endregion

    #region Nhom
    /// <summary>
    /// Cấu trúc dữ liệu nhóm chat trong Firebase:
    /// bao gồm thông tin nhóm, chủ nhóm, cờ thiết lập và danh sách thành viên.
    /// </summary>
    public class Nhom
    {
        /// <summary>
        /// ID nhóm (được dùng làm key chính trong Firebase).
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Tên hiển thị của nhóm. Nếu để trống sẽ fallback sang id.
        /// </summary>
        public string tenNhom { get; set; }

        /// <summary>
        /// Mô tả nhóm (không bắt buộc).
        /// </summary>
        public string moTa { get; set; }

        /// <summary>
        /// Tên (tài khoản) người tạo nhóm.
        /// Dùng để xác định quyền chủ nhóm.
        /// </summary>
        public string taoBoi { get; set; }

        /// <summary>
        /// Thời điểm tạo nhóm theo UnixTime (UTC).
        /// </summary>
        public long createdAt { get; set; }

        /// <summary>
        /// Chỉ cho admin (gold/silver) nhắn tin.
        /// False = mọi người đều gửi tin nhắn được.
        /// </summary>
        public bool AdminOnlyChat { get; set; } = false;

        /// <summary>
        /// Yêu cầu phê duyệt khi thêm thành viên mới.
        /// Hiện chỉ lưu cờ, chưa implement toàn phần workflow.
        /// </summary>
        public bool RequireApproval { get; set; } = false;

        /// <summary>
        /// Danh sách thành viên của nhóm.
        /// Key = tên user (Ten), 
        /// Value = thông tin thành viên (quyền, tier, mute).
        /// </summary>
        public Dictionary<string, GroupMemberInfo> thanhVien { get; set; }
            = new Dictionary<string, GroupMemberInfo>(StringComparer.OrdinalIgnoreCase);
    }
    #endregion
}
