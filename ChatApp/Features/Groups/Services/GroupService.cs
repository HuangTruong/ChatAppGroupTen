using ChatApp.Helpers;
using ChatApp.Models.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// GroupService: Service quản lý nhóm chat trên Firebase (REST).
    ///
    /// Nó phụ trách 4 nhóm việc chính:
    /// 1) Lấy nhóm của user (đọc groupsByUser/{userId})
    /// 2) Lấy metadata của nhóm (đọc groups/{groupId})
    /// 3) Tạo nhóm mới (tạo groups + members + groupsByUser)
    /// 4) Thêm thành viên + cập nhật preview tin nhắn cuối (LastMessage/LastMessageAt)
    ///
    /// Cấu trúc DB liên quan:
    /// - groups/{groupId}                 : metadata nhóm
    /// - groups/{groupId}/members/{uid}   : true (membership)
    /// - groupsByUser/{uid}/{groupId}     : true (nhóm mà user tham gia)
    /// </summary>
    public class GroupService
    {
        #region ====== BIẾN THÀNH VIÊN ======

        private readonly HttpService _http = new HttpService();

        #endregion

        #region ====== HỖ TRỢ TẠO URL FIREBASE ======

        /// <summary>
        /// Tạo URL Firebase Realtime Database dạng REST.
        /// - Nếu path rỗng: trỏ về root "/.json" để patch multi-location update.
        /// - Nếu có path: trỏ về "/{path}.json".
        /// </summary>
        private string Db(string path, string token = null)
        {
            string authQuery;
            if (string.IsNullOrEmpty(token))
            {
                authQuery = string.Empty;
            }
            else
            {
                authQuery = "?auth=" + token;
            }

            string root = FirebaseConfig.DatabaseUrl.TrimEnd('/');

            if (string.IsNullOrEmpty(path))
            {
                return root + "/.json" + authQuery;
            }

            return root + "/" + path + ".json" + authQuery;
        }

        #endregion

        #region ====== LẤY DANH SÁCH NHÓM ======

        /// <summary>
        /// Lấy danh sách groupId của 1 user từ node: groupsByUser/{userId}.
        /// </summary>
        public async Task<List<string>> GetGroupIdsOfUserAsync(string localId, string token = null)
        {
            string safeId = KeySanitizer.SafeKey(localId);
            string url = Db("groupsByUser/" + safeId, token);

            Dictionary<string, bool> map = await _http
                .GetAsync<Dictionary<string, bool>>(url)
                .ConfigureAwait(false);

            if (map == null || map.Count == 0)
            {
                return new List<string>();
            }

            return map.Keys.ToList();
        }

        /// <summary>
        /// Lấy metadata của một nhóm từ node: groups/{groupId}.
        /// </summary>
        public async Task<GroupInfo> GetGroupInfoAsync(string groupId, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return null;
            }

            string safeGroupId = KeySanitizer.SafeKey(groupId);
            string url = Db("groups/" + safeGroupId, token);

            GroupInfo g = await _http
                .GetAsync<GroupInfo>(url)
                .ConfigureAwait(false);

            if (g != null)
            {
                g.GroupId = safeGroupId;
            }

            return g;
        }

        /// <summary>
        /// Lấy toàn bộ nhóm mà user tham gia (groupId -> GroupInfo).
        /// - Đọc groupIds từ groupsByUser/{userId}
        /// - Với mỗi id, đọc metadata ở groups/{groupId}
        /// </summary>
        public async Task<Dictionary<string, GroupInfo>> GetMyGroupsAsync(string localId, string token = null)
        {
            List<string> ids = await GetGroupIdsOfUserAsync(localId, token).ConfigureAwait(false);

            Dictionary<string, GroupInfo> result = new Dictionary<string, GroupInfo>(StringComparer.Ordinal);

            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];

                GroupInfo g = await GetGroupInfoAsync(id, token).ConfigureAwait(false);
                if (g != null)
                {
                    result[g.GroupId] = g;
                }
                else
                {
                    // Nếu metadata bị thiếu, vẫn hiển thị groupId để bạn dễ debug
                    GroupInfo fallback = new GroupInfo();
                    fallback.GroupId = id;
                    fallback.Name = "Nhóm";
                    fallback.MemberCount = 0;

                    result[id] = fallback;
                }
            }

            return result;
        }

        #endregion

        #region ====== CẬP NHẬT TIN NHẮN CUỐI ======

        /// <summary>
        /// Cập nhật preview tin nhắn cuối của nhóm (LastMessage / LastMessageAt) trong groups/{groupId}.
        /// </summary>
        public async Task UpdateLastMessageAsync(string groupId, string lastMessage, long lastMessageAt, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            string gid = KeySanitizer.SafeKey(groupId);

            await _http
                .PatchAsync(Db("groups/" + gid, token), new
                {
                    LastMessage = lastMessage == null ? string.Empty : lastMessage,
                    LastMessageAt = lastMessageAt
                })
                .ConfigureAwait(false);
        }

        #endregion

        #region ====== TẠO NHÓM & LIÊN KẾT THÀNH VIÊN ======

        /// <summary>
        /// Tạo một nhóm mới.
        ///
        /// Mục tiêu: bấm "Tạo" là nhóm xuất hiện ngay, không cần đợi gửi tin nhắn đầu tiên.
        ///
        /// Thứ tự ghi:
        /// 1) Tạo metadata nhóm ở groups/{groupId}
        /// 2) Với mỗi thành viên:
        ///    - groups/{groupId}/members/{userId} = true
        ///    - groupsByUser/{userId}/{groupId} = true
        /// 3) Verify: creator phải thấy groupId trong groupsByUser/{creatorId}
        /// </summary>
        public async Task<string> CreateGroupAsync(
            string creatorLocalId,
            string groupName,
            List<string> memberIds,
            string token = null)
        {
            string creatorId = KeySanitizer.SafeKey(creatorLocalId);

            if (memberIds == null)
            {
                memberIds = new List<string>();
            }

            // Chuẩn hóa: unique + luôn có creator
            HashSet<string> normalized = new HashSet<string>(StringComparer.Ordinal);

            if (!string.IsNullOrEmpty(creatorId))
            {
                normalized.Add(creatorId);
            }

            for (int i = 0; i < memberIds.Count; i++)
            {
                string safe = KeySanitizer.SafeKey(memberIds[i]);
                if (!string.IsNullOrEmpty(safe))
                {
                    normalized.Add(safe);
                }
            }

            if (normalized.Count == 0)
            {
                throw new Exception("Không có thành viên hợp lệ để tạo nhóm.");
            }

            string groupId = KeySanitizer.SafeKey(Guid.NewGuid().ToString("N"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            GroupInfo group = new GroupInfo();
            group.GroupId = groupId;
            group.Name = string.IsNullOrWhiteSpace(groupName) ? "Nhóm mới" : groupName.Trim();
            group.CreatedBy = creatorId;
            group.CreatedAt = now;
            group.MemberCount = normalized.Count;
            group.LastMessage = string.Empty;
            group.LastMessageAt = 0;

            // 1) Tạo metadata group trước
            await _http
                .PatchAsync(Db("groups/" + groupId, token), group)
                .ConfigureAwait(false);

            // 2) Ghi members + groupsByUser cho từng user (best-effort, dễ debug)
            foreach (string uid in normalized)
            {
                await EnsureMembershipLinkAsync(groupId, uid, token).ConfigureAwait(false);
            }

            // 3) Verify: creator phải thấy group trong groupsByUser
            try
            {
                Dictionary<string, bool> map = await _http
                    .GetAsync<Dictionary<string, bool>>(Db("groupsByUser/" + creatorId, token))
                    .ConfigureAwait(false);

                if (map == null || !map.ContainsKey(groupId))
                {
                    throw new Exception("Không ghi được groupsByUser. Kiểm tra Firebase Rules quyền ghi node 'groupsByUser'.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Tạo nhóm xong nhưng verify groupsByUser thất bại: " + ex.Message);
            }

            return groupId;
        }

        /// <summary>
        /// Đảm bảo mapping membership tối thiểu:
        /// - groups/{groupId}/members/{userId} = true
        /// - groupsByUser/{userId}/{groupId} = true
        /// (không tăng MemberCount ở đây để tránh đếm sai)
        /// </summary>
        public async Task EnsureMembershipLinkAsync(string groupId, string userId, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("userId rỗng.");
            }

            string gid = KeySanitizer.SafeKey(groupId);
            string uid = KeySanitizer.SafeKey(userId);

            // Patch members
            Dictionary<string, object> memPatch = new Dictionary<string, object>();
            memPatch[uid] = true;

            await _http
                .PatchAsync(Db("groups/" + gid + "/members", token), memPatch)
                .ConfigureAwait(false);

            // Patch groupsByUser
            Dictionary<string, object> byUserPatch = new Dictionary<string, object>();
            byUserPatch[gid] = true;

            await _http
                .PatchAsync(Db("groupsByUser/" + uid, token), byUserPatch)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Thêm 1 thành viên vào nhóm:
        /// - Ghi 2 đường link (members + groupsByUser) bằng patch root (multi-location)
        /// - Sau đó tăng MemberCount (best-effort)
        /// </summary>
        public async Task AddMemberAsync(string groupId, string memberLocalId, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            string gid = KeySanitizer.SafeKey(groupId);
            string uid = KeySanitizer.SafeKey(memberLocalId);

            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("memberLocalId rỗng.");
            }

            // Multi-location update
            Dictionary<string, object> updates = new Dictionary<string, object>();
            updates["groups/" + gid + "/members/" + uid] = true;
            updates["groupsByUser/" + uid + "/" + gid] = true;

            await _http
                .PatchAsync(Db(string.Empty, token), updates)
                .ConfigureAwait(false);

            // Tăng MemberCount (best-effort)
            try
            {
                GroupInfo g = await GetGroupInfoAsync(gid, token).ConfigureAwait(false);
                if (g != null)
                {
                    int cnt = g.MemberCount;
                    if (cnt < 0)
                    {
                        cnt = 0;
                    }
                    cnt++;

                    await _http
                        .PatchAsync(Db("groups/" + gid, token), new { MemberCount = cnt })
                        .ConfigureAwait(false);
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion
    }
}
