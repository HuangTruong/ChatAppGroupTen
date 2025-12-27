using ChatApp.Helpers;
using ChatApp.Models.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Service quản lý nhóm chat:
    /// - Tạo nhóm
    /// - Thêm thành viên
    /// - Lấy danh sách nhóm của user
    /// - Lấy metadata nhóm
    /// </summary>
    public class GroupService
    {
        private readonly HttpService _http = new HttpService();

        #region ====== URL HELPERS ======

        /// <summary>
        /// Helper: Tạo URL Firebase Realtime Database.
        /// </summary>
        private string Db(string path, string token = null)
        {
            string auth = string.IsNullOrEmpty(token) ? string.Empty : ("?auth=" + token);

            // Root patch multi-location update
            if (string.IsNullOrEmpty(path))
            {
                return FirebaseConfig.DatabaseUrl.TrimEnd('/') + "/.json" + auth;
            }

            return FirebaseConfig.DatabaseUrl.TrimEnd('/') + "/" + path + ".json" + auth;
        }

        #endregion

        #region ====== GET GROUPS ======

        /// <summary>
        /// Lấy danh sách groupId của 1 user từ node groupsByUser/{userId}.
        /// </summary>
        public async Task<List<string>> GetGroupIdsOfUserAsync(string localId, string token = null)
        {
            string safeId = KeySanitizer.SafeKey(localId);
            string url = Db("groupsByUser/" + safeId, token);

            var map = await _http.GetAsync<Dictionary<string, bool>>(url).ConfigureAwait(false);

            if (map == null || map.Count == 0)
            {
                return new List<string>();
            }

            return map.Keys.ToList();
        }

        /// <summary>
        /// Lấy metadata của một nhóm.
        /// </summary>
        public async Task<GroupInfo> GetGroupInfoAsync(string groupId, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return null;
            }

            string safeGroupId = KeySanitizer.SafeKey(groupId);
            string url = Db("groups/" + safeGroupId, token);

            var g = await _http.GetAsync<GroupInfo>(url).ConfigureAwait(false);
            if (g != null)
            {
                g.GroupId = safeGroupId;
            }

            return g;
        }

        /// <summary>
        /// Lấy toàn bộ nhóm mà user tham gia (groupId -> GroupInfo).
        /// </summary>
        public async Task<Dictionary<string, GroupInfo>> GetMyGroupsAsync(string localId, string token = null)
        {
            var ids = await GetGroupIdsOfUserAsync(localId, token).ConfigureAwait(false);

            var result = new Dictionary<string, GroupInfo>();
            foreach (string id in ids)
            {
                var g = await GetGroupInfoAsync(id, token).ConfigureAwait(false);
                if (g != null)
                {
                    result[g.GroupId] = g;
                }
                else
                {
                    // Nếu metadata bị thiếu, vẫn cho hiện groupId để debug
                    result[id] = new GroupInfo
                    {
                        GroupId = id,
                        Name = "Nhóm",
                        MemberCount = 0
                    };
                }
            }

            return result;
        }

        #endregion

        #region ====== UPDATE LAST MESSAGE ======

        /// <summary>
        /// Cập nhật preview tin nhắn cuối của nhóm (LastMessage / LastMessageAt).
        /// </summary>
        public async Task UpdateLastMessageAsync(string groupId, string lastMessage, long lastMessageAt, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            string gid = KeySanitizer.SafeKey(groupId);

            await _http.PatchAsync(Db("groups/" + gid, token), new
            {
                LastMessage = lastMessage ?? string.Empty,
                LastMessageAt = lastMessageAt
            }).ConfigureAwait(false);
        }

        #endregion

        #region ====== CREATE / MEMBER ======

        /// <summary>
        /// Tạo một nhóm mới.
        /// Cấu trúc DB:
        /// - groups/{groupId}
        /// - groups/{groupId}/members/{userId} = true
        /// - groupsByUser/{userId}/{groupId} = true
        /// </summary>

        //create group có avatar
        public async Task<string> CreateGroupAsync(string creatorLocalId, string groupName, 
            List<string> memberIds, string avatarBase64, string token = null)
        {
            string creatorId = KeySanitizer.SafeKey(creatorLocalId);
            if (memberIds == null) memberIds = new List<string>();

            var normalized = new HashSet<string>(StringComparer.Ordinal);
            normalized.Add(creatorId);

            foreach (string m in memberIds)
            {
                string safe = KeySanitizer.SafeKey(m);
                if (!string.IsNullOrEmpty(safe)) normalized.Add(safe);
            }

            string groupId = KeySanitizer.SafeKey(Guid.NewGuid().ToString("N"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var membersMap = new Dictionary<string, bool>();
            foreach (string uid in normalized) membersMap[uid] = true;

            // Tạo nhóm + members + avatar NGAY LẬP TỨC
            await _http.PatchAsync(Db("groups/" + groupId, token), new
            {
                GroupId = groupId,
                Name = string.IsNullOrWhiteSpace(groupName) ? "Nhóm mới" : groupName.Trim(),
                CreatedBy = creatorId,
                CreatedAt = now,
                MemberCount = normalized.Count,
                LastMessage = string.Empty,
                LastMessageAt = 0,
                avatar = string.IsNullOrWhiteSpace(avatarBase64) ? string.Empty : avatarBase64,
                members = membersMap
            }).ConfigureAwait(false);

            // Creator thấy nhóm ngay
            await _http.PatchAsync(Db("groupsByUser/" + creatorId, token),
                new Dictionary<string, bool> { { groupId, true } }).ConfigureAwait(false);

            // Best-effort cho các member khác (tuỳ rules)
            foreach (string uid in normalized)
            {
                if (string.Equals(uid, creatorId, StringComparison.Ordinal)) continue;
                try
                {
                    await _http.PatchAsync(Db("groupsByUser/" + uid, token),
                        new Dictionary<string, bool> { { groupId, true } }).ConfigureAwait(false);
                }
                catch { }
            }

            return groupId;
        }

        /// <summary>
        /// Thêm thành viên vào nhóm.
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

            var updates = new Dictionary<string, object>();
            updates["groups/" + gid + "/members/" + uid] = true;
            updates["groupsByUser/" + uid + "/" + gid] = true;

            await _http.PatchAsync(Db(string.Empty, token), updates).ConfigureAwait(false);

            // Tăng memberCount (best-effort)
            try
            {
                GroupInfo g = await GetGroupInfoAsync(gid, token).ConfigureAwait(false);
                if (g != null)
                {
                    int cnt = g.MemberCount;
                    if (cnt < 0) cnt = 0;
                    cnt++;

                    await _http.PatchAsync(Db("groups/" + gid, token), new
                    {
                        MemberCount = cnt
                    }).ConfigureAwait(false);
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ====== GROUP MEMBERS / UPDATE NAME ======

        /// <summary>
        /// Lấy danh sách members của nhóm: groups/{gid}/members.
        /// </summary>
        public async Task<Dictionary<string, bool>> GetMemberMapAsync(string groupId, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return new Dictionary<string, bool>();

            string gid = KeySanitizer.SafeKey(groupId);
            var map = await _http.GetAsync<Dictionary<string, bool>>(Db("groups/" + gid + "/members", token))
                                 .ConfigureAwait(false);

            return map ?? new Dictionary<string, bool>();
        }

        /// <summary>
        /// Đổi tên nhóm (patch groups/{gid}/Name).
        /// </summary>
        public async Task UpdateGroupNameAsync(string groupId, string newName, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId)) throw new ArgumentException("groupId rỗng.");

            string name = (newName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên nhóm rỗng.");

            string gid = KeySanitizer.SafeKey(groupId);

            await _http.PatchAsync(Db("groups/" + gid, token), new
            {
                Name = name
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Thêm nhiều thành viên vào nhóm:
        /// - Patch groups/{gid}/members/{uid} = true (merge)
        /// - Best-effort patch groupsByUser/{uid}/{gid} = true
        /// - Recount và cập nhật MemberCount cho đúng
        /// </summary>
        /// <returns>Số member mới được thêm</returns>
        public async Task<int> AddMembersAsync(string groupId, IEnumerable<string> memberLocalIds, string token = null)
        {
            if (string.IsNullOrWhiteSpace(groupId)) throw new ArgumentException("groupId rỗng.");

            string gid = KeySanitizer.SafeKey(groupId);
            if (memberLocalIds == null) return 0;

            Dictionary<string, bool> current = await GetMemberMapAsync(gid, token).ConfigureAwait(false);

            // Lọc danh sách cần thêm (unique + chưa có)
            Dictionary<string, bool> addMap = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (string raw in memberLocalIds)
            {
                string uid = KeySanitizer.SafeKey(raw);
                if (string.IsNullOrEmpty(uid)) continue;

                bool existed;
                if (current != null && current.TryGetValue(uid, out existed) && existed) continue;

                if (!addMap.ContainsKey(uid)) addMap[uid] = true;
            }

            if (addMap.Count == 0) return 0;

            // 1) merge members
            await _http.PatchAsync(Db("groups/" + gid + "/members", token), addMap).ConfigureAwait(false);

            // 2) best-effort groupsByUser (tuỳ rules)
            foreach (KeyValuePair<string, bool> kv in addMap)
            {
                string uid = kv.Key;
                try
                {
                    await _http.PatchAsync(Db("groupsByUser/" + uid, token),
                        new Dictionary<string, bool> { { gid, true } }).ConfigureAwait(false);
                }
                catch { }
            }

            // 3) recount MemberCount cho chính xác
            try
            {
                Dictionary<string, bool> after = await GetMemberMapAsync(gid, token).ConfigureAwait(false);
                int count = after != null ? after.Count : 0;

                await _http.PatchAsync(Db("groups/" + gid, token), new
                {
                    MemberCount = count
                }).ConfigureAwait(false);
            }
            catch { }

            return addMap.Count;
        }

        #endregion

        #region ===== AVATAR GROUP =====
        public async Task<string> GetAvatarGroupAsync(string groupId, string token = null)
        {
            string key = KeySanitizer.SafeKey(groupId);
            return await _http.GetAsync<string>(Db(string.Format("groups/{0}/avatar", key), token))
                              .ConfigureAwait(false);
        }

        public async Task UpdateAvatarAsync(string groupId, string avatarBase64, string token = null)
        {
            string key = KeySanitizer.SafeKey(groupId);

            // Ghi đúng vào metadata của group
            await _http.PatchAsync(Db(string.Format("groups/{0}", key), token), new
            {
                avatar = avatarBase64 ?? string.Empty
            }).ConfigureAwait(false);
        }

        #endregion


    }
}
