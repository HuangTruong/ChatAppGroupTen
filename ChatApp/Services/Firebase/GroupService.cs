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

            // Normalize + unique + luôn có creator
            var normalized = new HashSet<string>(StringComparer.Ordinal);
            normalized.Add(creatorId);

            foreach (string m in memberIds)
            {
                string safe = KeySanitizer.SafeKey(m);
                if (!string.IsNullOrEmpty(safe))
                {
                    normalized.Add(safe);
                }
            }

            string groupId = KeySanitizer.SafeKey(Guid.NewGuid().ToString("N"));

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            GroupInfo group = new GroupInfo
            {
                GroupId = groupId,
                Name = string.IsNullOrWhiteSpace(groupName) ? "Nhóm mới" : groupName.Trim(),
                CreatedBy = creatorId,
                CreatedAt = now,
                MemberCount = normalized.Count,
                LastMessage = string.Empty,
                LastMessageAt = 0
            };

            // Multi-location update (atomic)
            var updates = new Dictionary<string, object>();

            updates["groups/" + groupId] = group;

            foreach (string uid in normalized)
            {
                updates["groups/" + groupId + "/members/" + uid] = true;
                updates["groupsByUser/" + uid + "/" + groupId] = true;
            }

            await _http.PatchAsync(Db(string.Empty, token), updates).ConfigureAwait(false);

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
    }
}
