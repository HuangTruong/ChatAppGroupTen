using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Models.Chat;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace ChatApp.Services.Chat
{
    /// <summary>
    /// Service quản lý nhóm chat trên Firebase:
    /// - Tạo / xoá nhóm, lấy danh sách nhóm.
    /// - Quản lý thành viên (thêm / xoá / cập nhật quyền, chuyển chủ nhóm).
    /// - Gửi / tải tin nhắn nhóm, mute / unmute thành viên.
    /// - Cài đặt cờ <c>AdminOnlyChat</c>, <c>RequireApproval</c>.
    /// </summary>
    public class GroupService
    {
        #region ======== Hằng số / Trường ========

        /// <summary>
        /// Client Firebase dùng để thao tác dữ liệu.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Node gốc lưu thông tin nhóm.
        /// </summary>
        private const string GroupsRoot = "groups";

        /// <summary>
        /// Node gốc lưu tin nhắn nhóm.
        /// </summary>
        private const string GroupMessagesRoot = "groupMessages";

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo <see cref="GroupService"/> với client Firebase đã cấu hình.
        /// </summary>
        /// <param name="firebase">Client Firebase.</param>
        public GroupService(IFirebaseClient firebase)
        {
            if (firebase == null) throw new ArgumentNullException("firebase");
            _firebase = firebase;
        }

        #endregion

        #region ======== Lấy thông tin nhóm ========

        /// <summary>
        /// Lấy toàn bộ danh sách nhóm trong hệ thống.
        /// </summary>
        /// <returns>
        /// Dictionary với key là <c>groupId</c>, value là <see cref="Nhom"/>.
        /// Nếu không có dữ liệu sẽ trả về dictionary rỗng.
        /// </returns>
        public async Task<Dictionary<string, Nhom>> GetAllAsync()
        {
            var res = await _firebase.GetAsync(GroupsRoot);
            var data = res.ResultAs<Dictionary<string, Nhom>>();
            return data ?? new Dictionary<string, Nhom>();
        }

        /// <summary>
        /// Lấy thông tin 1 nhóm theo <paramref name="groupId"/>.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <returns>
        /// Đối tượng <see cref="Nhom"/> hoặc <c>null</c> nếu nhóm không tồn tại.
        /// </returns>
        public async Task<Nhom> GetAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return null;

            var res = await _firebase.GetAsync(GroupsRoot + "/" + groupId);
            return res.Body == "null" ? null : res.ResultAs<Nhom>();
        }

        #endregion

        #region ======== Tạo / Xoá / Cập nhật nhóm ========

        /// <summary>
        /// Tạo nhóm mới:
        /// - Sinh ID ngẫu nhiên dạng GUID (chuỗi 32 ký tự).
        /// - Chủ nhóm mặc định là <paramref name="owner"/> (tier "gold").
        /// - Các thành viên khác tier "member".
        /// </summary>
        /// <param name="tenNhom">Tên nhóm. Nếu rỗng sẽ dùng tên mặc định "Nhóm của {owner}".</param>
        /// <param name="owner">Tên chủ nhóm (bắt buộc).</param>
        /// <param name="members">Danh sách thành viên thêm vào cùng chủ nhóm.</param>
        /// <returns>ID nhóm vừa tạo.</returns>
        public async Task<string> CreateGroupAsync(string tenNhom, string owner, IEnumerable<string> members)
        {
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Owner không hợp lệ", "owner");

            tenNhom = tenNhom == null ? null : tenNhom.Trim();
            if (string.IsNullOrEmpty(tenNhom))
                tenNhom = "Nhóm của " + owner;

            string id = Guid.NewGuid().ToString("N");

            var allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allMembers.Add(owner);

            if (members != null)
            {
                foreach (var m in members)
                {
                    if (!string.IsNullOrWhiteSpace(m))
                    {
                        allMembers.Add(m.Trim());
                    }
                }
            }

            var thanhVien = new Dictionary<string, GroupMemberInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in allMembers)
            {
                bool isOwner = string.Equals(u, owner, StringComparison.OrdinalIgnoreCase);

                thanhVien[u] = new GroupMemberInfo
                {
                    IsAdmin = isOwner,
                    Tier = isOwner ? "gold" : "member"
                };
            }

            var group = new Nhom
            {
                id = id,
                tenNhom = tenNhom,
                taoBoi = owner,
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                thanhVien = thanhVien
            };

            await _firebase.SetAsync(GroupsRoot + "/" + id, group);
            return id;
        }

        /// <summary>
        /// Thêm một thành viên vào nhóm với quyền và tier xác định.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="userName">Tên thành viên cần thêm.</param>
        /// <param name="isAdmin">Có phải admin hay không.</param>
        /// <param name="tier">Tier hiển thị ("gold", "silver", "member").</param>
        public async Task AddMemberAsync(string groupId, string userName, bool isAdmin = false, string tier = "member")
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var member = new GroupMemberInfo
            {
                IsAdmin = isAdmin,
                Tier = string.IsNullOrWhiteSpace(tier) ? "member" : tier
            };

            await _firebase.SetAsync(GroupsRoot + "/" + groupId + "/thanhVien/" + userName, member);
        }

        /// <summary>
        /// Xoá một thành viên khỏi nhóm.
        /// Nếu thành viên đang là admin, kiểm tra để đảm bảo vẫn còn ít nhất một admin trong nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="userName">Tên thành viên cần xoá.</param>
        public async Task RemoveMemberAsync(string groupId, string userName)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null)
                return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info))
                return;

            if (info.IsAdmin)
            {
                int adminCount = group.thanhVien.Values.Count(delegate (GroupMemberInfo m)
                {
                    return m != null && m.IsAdmin;
                });

                if (adminCount <= 1)
                    throw new InvalidOperationException("Nhóm phải còn ít nhất một quản trị viên.");
            }

            await _firebase.DeleteAsync(GroupsRoot + "/" + groupId + "/thanhVien/" + userName);
        }

        /// <summary>
        /// Cập nhật quyền và tier của một thành viên trong nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="userName">Tên thành viên.</param>
        /// <param name="isAdmin">Có phải admin hay không.</param>
        /// <param name="tier">Tier hiển thị ("gold", "silver", "member").</param>
        public async Task UpdateMemberRoleAsync(string groupId, string userName, bool isAdmin, string tier)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null)
                return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info))
                return;

            info.IsAdmin = isAdmin;
            info.Tier = string.IsNullOrWhiteSpace(tier) ? "member" : tier;

            await _firebase.SetAsync(GroupsRoot + "/" + groupId + "/thanhVien/" + userName, info);
        }

        /// <summary>
        /// Xoá hoàn toàn một nhóm và toàn bộ tin nhắn trong nhóm đó.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        public async Task DeleteGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            await _firebase.DeleteAsync(GroupsRoot + "/" + groupId);
            await _firebase.DeleteAsync(GroupMessagesRoot + "/" + groupId);
        }

        #endregion

        #region ======== Tin nhắn nhóm ========

        /// <summary>
        /// Tải toàn bộ tin nhắn của một nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <returns>Danh sách <see cref="TinNhan"/> với <c>laNhom = true</c>.</returns>
        public async Task<List<TinNhan>> LoadGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return new List<TinNhan>();

            var res = await _firebase.GetAsync(GroupMessagesRoot + "/" + groupId);
            var dict = res.ResultAs<Dictionary<string, TinNhan>>() ??
                       new Dictionary<string, TinNhan>();

            var list = new List<TinNhan>();

            foreach (var kv in dict)
            {
                if (kv.Value == null)
                    continue;

                if (string.IsNullOrEmpty(kv.Value.id))
                {
                    kv.Value.id = kv.Key;
                }

                kv.Value.laNhom = true;
                list.Add(kv.Value);
            }

            // Có thể sort theo thời gian nếu cần, hiện tại giữ nguyên thứ tự Firebase trả về
            return list;
        }

        /// <summary>
        /// Gửi tin nhắn vào nhóm:
        /// - Tạo <see cref="TinNhan"/> với <c>laNhom = true</c>.
        /// - Push để sinh ID trên Firebase.
        /// - Cập nhật lại trường <c>id</c> cho bản ghi vừa push.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="guiBoi">Tên người gửi.</param>
        /// <param name="noiDung">Nội dung tin nhắn.</param>
        /// <returns>Đối tượng <see cref="TinNhan"/> đã gán ID, hoặc null nếu tham số không hợp lệ.</returns>
        public async Task<TinNhan> SendGroupAsync(string groupId, string guiBoi, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(groupId) ||
                string.IsNullOrWhiteSpace(guiBoi) ||
                string.IsNullOrWhiteSpace(noiDung))
            {
                return null;
            }

            var msg = new TinNhan
            {
                guiBoi = guiBoi,
                noiDung = noiDung,
                thoiGian = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                laNhom = true
            };

            PushResponse res = await _firebase.PushAsync(GroupMessagesRoot + "/" + groupId, msg);
            string id = res.Result.name;

            msg.id = id;
            await _firebase.UpdateAsync(GroupMessagesRoot + "/" + groupId + "/" + id, new { id = id });

            return msg;
        }

        /// <summary>
        /// Lấy path Firebase chứa tin nhắn nhóm để dùng cho stream realtime.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <returns>Chuỗi path node tin nhắn nhóm.</returns>
        public string GetGroupMessagesPath(string groupId)
        {
            return GroupMessagesRoot + "/" + groupId;
        }

        #endregion

        #region ======== Cờ AdminOnlyChat / RequireApproval ========

        /// <summary>
        /// Bật/tắt chế độ chỉ admin được gửi tin nhắn trong nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="adminOnlyChat"><c>true</c> nếu chỉ admin được chat.</param>
        public async Task SetAdminOnlyChatAsync(string groupId, bool adminOnlyChat)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            await _firebase.UpdateAsync(GroupsRoot + "/" + groupId, new { AdminOnlyChat = adminOnlyChat });
        }

        /// <summary>
        /// Bật/tắt chế độ yêu cầu phê duyệt thành viên mới.
        /// (Hiện tại chỉ lưu cờ, logic join có thể triển khai sau.)
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="requireApproval">Trạng thái cờ RequireApproval.</param>
        public async Task SetRequireApprovalAsync(string groupId, bool requireApproval)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            await _firebase.UpdateAsync(GroupsRoot + "/" + groupId, new { RequireApproval = requireApproval });
        }

        /// <summary>
        /// Cập nhật tên hiển thị của nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="newName">Tên mới.</param>
        public async Task UpdateGroupNameAsync(string groupId, string newName)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return;
            if (string.IsNullOrWhiteSpace(newName))
                return;

            await _firebase.UpdateAsync(GroupsRoot + "/" + groupId, new { tenNhom = newName });
        }

        #endregion

        #region ======== Mute / Unmute thành viên ========

        /// <summary>
        /// Cấm chat (mute) một thành viên trong nhóm trong khoảng <paramref name="duration"/>.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="userName">Tên thành viên.</param>
        /// <param name="duration">Thời gian cấm chat.</param>
        public async Task MuteMemberAsync(string groupId, string userName, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null)
                return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info) || info == null)
                return;

            long until = DateTimeOffset.UtcNow.Add(duration).ToUnixTimeSeconds();
            info.MutedUntil = until;

            await _firebase.SetAsync(GroupsRoot + "/" + groupId + "/thanhVien/" + userName, info);
        }

        /// <summary>
        /// Bỏ cấm chat (unmute) một thành viên trong nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="userName">Tên thành viên.</param>
        public async Task UnmuteMemberAsync(string groupId, string userName)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null)
                return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info) || info == null)
                return;

            info.MutedUntil = 0;

            await _firebase.SetAsync(GroupsRoot + "/" + groupId + "/thanhVien/" + userName, info);
        }

        #endregion

        #region ======== Chuyển quyền chủ nhóm ========

        /// <summary>
        /// Nhượng quyền chủ nhóm từ <paramref name="currentOwner"/> sang <paramref name="newOwner"/>:
        /// - Kiểm tra chủ hiện tại có đúng là <c>group.taoBoi</c>.
        /// - Đảm bảo <paramref name="newOwner"/> đang là thành viên nhóm.
        /// - Chủ cũ trở thành member thường (tier "member", <c>IsAdmin = false</c>).
        /// - Chủ mới trở thành admin vàng (tier "gold", <c>IsAdmin = true</c>).
        /// </summary>
        /// <param name="groupId">ID nhóm.</param>
        /// <param name="currentOwner">Chủ nhóm hiện tại.</param>
        /// <param name="newOwner">Thành viên sẽ nhận quyền chủ nhóm.</param>
        public async Task TransferOwnershipAsync(string groupId, string currentOwner, string newOwner)
        {
            if (string.IsNullOrWhiteSpace(groupId) ||
                string.IsNullOrWhiteSpace(currentOwner) ||
                string.IsNullOrWhiteSpace(newOwner))
            {
                return;
            }

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null)
                return;

            if (!string.Equals(group.taoBoi, currentOwner, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Chỉ chủ nhóm mới được nhượng quyền.");

            GroupMemberInfo newInfo;
            if (!group.thanhVien.TryGetValue(newOwner, out newInfo) || newInfo == null)
                throw new InvalidOperationException("Thành viên mới phải đang ở trong nhóm.");

            GroupMemberInfo oldInfo;
            if (!group.thanhVien.TryGetValue(currentOwner, out oldInfo) || oldInfo == null)
            {
                oldInfo = new GroupMemberInfo();
                group.thanhVien[currentOwner] = oldInfo;
            }

            // Chủ cũ thành member thường
            oldInfo.IsAdmin = false;
            oldInfo.Tier = "member";

            // Chủ mới: admin vàng
            newInfo.IsAdmin = true;
            newInfo.Tier = "gold";

            group.taoBoi = newOwner;

            await _firebase.SetAsync(GroupsRoot + "/" + groupId, group);
        }

        #endregion
    }
}
