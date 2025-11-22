using ChatApp.Models.Chat;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Chat
{
    public class GroupService
    {
        private readonly IFirebaseClient _firebase;
        private const string GroupsRoot = "groups";
        private const string GroupMessagesRoot = "groupMessages";

        public GroupService(IFirebaseClient firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Lấy tất cả nhóm
        public async Task<Dictionary<string, Nhom>> GetAllAsync()
        {
            var res = await _firebase.GetAsync(GroupsRoot);
            var data = res.ResultAs<Dictionary<string, Nhom>>();
            return data ?? new Dictionary<string, Nhom>();
        }

        // Lấy 1 nhóm theo id
        public async Task<Nhom> GetAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return null;
            var res = await _firebase.GetAsync($"{GroupsRoot}/{groupId}");
            return res.Body == "null" ? null : res.ResultAs<Nhom>();
        }

        // Tạo nhóm mới
        public async Task<string> CreateGroupAsync(string tenNhom, string owner, IEnumerable<string> members)
        {
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Owner không hợp lệ", nameof(owner));

            tenNhom = tenNhom?.Trim();
            if (string.IsNullOrEmpty(tenNhom))
                tenNhom = $"Nhóm của {owner}";

            var id = Guid.NewGuid().ToString("N");

            var allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allMembers.Add(owner);

            if (members != null)
            {
                foreach (var m in members)
                {
                    if (!string.IsNullOrWhiteSpace(m))
                        allMembers.Add(m.Trim());
                }
            }

            var thanhVien = new Dictionary<string, GroupMemberInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in allMembers)
            {
                thanhVien[u] = new GroupMemberInfo
                {
                    IsAdmin = string.Equals(u, owner, StringComparison.OrdinalIgnoreCase),
                    Tier = string.Equals(u, owner, StringComparison.OrdinalIgnoreCase) ? "gold" : "member"
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

            await _firebase.SetAsync($"{GroupsRoot}/{id}", group);
            return id;
        }

        // Thêm thành viên
        public async Task AddMemberAsync(string groupId, string userName, bool isAdmin = false, string tier = "member")
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var member = new GroupMemberInfo
            {
                IsAdmin = isAdmin,
                Tier = string.IsNullOrWhiteSpace(tier) ? "member" : tier
            };

            await _firebase.SetAsync($"{GroupsRoot}/{groupId}/thanhVien/{userName}", member);
        }

        // Xoá thành viên (có kiểm tra không xoá hết admin)
        public async Task RemoveMemberAsync(string groupId, string userName)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group?.thanhVien == null) return;

            if (!group.thanhVien.TryGetValue(userName, out var info))
                return;

            if (info.IsAdmin)
            {
                int adminCount = group.thanhVien.Values.Count(m => m.IsAdmin);
                if (adminCount <= 1)
                    throw new InvalidOperationException("Nhóm phải còn ít nhất một quản trị viên.");
            }

            await _firebase.DeleteAsync($"{GroupsRoot}/{groupId}/thanhVien/{userName}");
        }

        // Cập nhật quyền + tier
        public async Task UpdateMemberRoleAsync(string groupId, string userName, bool isAdmin, string tier)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group?.thanhVien == null) return;

            if (!group.thanhVien.TryGetValue(userName, out var info))
                return;

            info.IsAdmin = isAdmin;
            info.Tier = string.IsNullOrWhiteSpace(tier) ? "member" : tier;

            await _firebase.SetAsync($"{GroupsRoot}/{groupId}/thanhVien/{userName}", info);
        }

        // Xoá cả nhóm
        public async Task DeleteGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            await _firebase.DeleteAsync($"{GroupsRoot}/{groupId}");
            await _firebase.DeleteAsync($"{GroupMessagesRoot}/{groupId}");
        }

        // ====== Tin nhắn nhóm ======

        public async Task<List<TinNhan>> LoadGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return new List<TinNhan>();

            var res = await _firebase.GetAsync($"{GroupMessagesRoot}/{groupId}");
            var dict = res.ResultAs<Dictionary<string, TinNhan>>()
                       ?? new Dictionary<string, TinNhan>();

            var list = new List<TinNhan>();
            foreach (var kv in dict)
            {
                if (kv.Value == null) continue;
                if (kv.Value.id == null || kv.Value.id == "")
                {
                    kv.Value.id = kv.Key;
                }
                kv.Value.laNhom = true;
                list.Add(kv.Value);
            }

            // tuỳ bạn sort theo thời gian nếu TinNhan có trường thời gian
            return list;
        }

        // Gửi tin nhắn nhóm
        public async Task<TinNhan> SendGroupAsync(string groupId, string guiBoi, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(groupId) ||
                string.IsNullOrWhiteSpace(guiBoi) ||
                string.IsNullOrWhiteSpace(noiDung))
                return null;

            var msg = new TinNhan
            {
                guiBoi = guiBoi,
                noiDung = noiDung,
                thoiGian = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                laNhom = true,
                laEmoji = false,
                emojiKey = null
            };

            PushResponse res = await _firebase.PushAsync($"{GroupMessagesRoot}/{groupId}", msg);
            string id = res.Result.name;

            msg.id = id;
            await _firebase.UpdateAsync($"{GroupMessagesRoot}/{groupId}/{id}", new { id });

            return msg;
        }

        // Gửi emoji nhóm
        public async Task<TinNhan> SendGroupEmojiAsync(string groupId, string guiBoi, string emojiKey)
        {
            if (string.IsNullOrWhiteSpace(groupId) ||
                string.IsNullOrWhiteSpace(guiBoi) ||
                string.IsNullOrWhiteSpace(emojiKey))
                return null;

            var msg = new TinNhan
            {
                guiBoi = guiBoi,
                // fallback text
                noiDung = $"[emoji:{emojiKey}]",
                thoiGian = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                laNhom = true,
                laEmoji = true,
                emojiKey = emojiKey
            };

            PushResponse res = await _firebase.PushAsync($"{GroupMessagesRoot}/{groupId}", msg);
            string id = res.Result.name;

            msg.id = id;
            await _firebase.UpdateAsync($"{GroupMessagesRoot}/{groupId}/{id}", new { id });

            return msg;
        }


        // Để controller dùng đúng path realtime
        public string GetGroupMessagesPath(string groupId)
        {
            return $"{GroupMessagesRoot}/{groupId}";
        }
        public async Task SetAdminOnlyChatAsync(string groupId, bool adminOnlyChat)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;
            await _firebase.UpdateAsync($"{GroupsRoot}/{groupId}", new { AdminOnlyChat = adminOnlyChat });
        }

        public async Task SetRequireApprovalAsync(string groupId, bool requireApproval)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;
            await _firebase.UpdateAsync($"{GroupsRoot}/{groupId}", new { RequireApproval = requireApproval });
        }

        public async Task UpdateGroupNameAsync(string groupId, string newName)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;
            if (string.IsNullOrWhiteSpace(newName)) return;

            await _firebase.UpdateAsync($"{GroupsRoot}/{groupId}", new { tenNhom = newName });
        }

        public async Task MuteMemberAsync(string groupId, string userName, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null) return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info) || info == null)
                return;

            long until = DateTimeOffset.UtcNow.Add(duration).ToUnixTimeSeconds();
            info.MutedUntil = until;

            await _firebase.SetAsync($"{GroupsRoot}/{groupId}/thanhVien/{userName}", info);
        }

        public async Task UnmuteMemberAsync(string groupId, string userName)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userName))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null) return;

            GroupMemberInfo info;
            if (!group.thanhVien.TryGetValue(userName, out info) || info == null)
                return;

            info.MutedUntil = 0;

            await _firebase.SetAsync($"{GroupsRoot}/{groupId}/thanhVien/{userName}", info);
        }

        public async Task TransferOwnershipAsync(string groupId, string currentOwner, string newOwner)
        {
            if (string.IsNullOrWhiteSpace(groupId) ||
                string.IsNullOrWhiteSpace(currentOwner) ||
                string.IsNullOrWhiteSpace(newOwner))
                return;

            var group = await GetAsync(groupId);
            if (group == null || group.thanhVien == null) return;

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

            await _firebase.SetAsync($"{GroupsRoot}/{groupId}", group);
        }
    }
}
