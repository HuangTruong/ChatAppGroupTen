using ChatApp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Service xử lý tin nhắn nhóm:
    /// - Send text / file
    /// - Load history / load incremental
    /// Node: groupMessages/{groupId}/{messageId}
    /// </summary>
    public class GroupMessageService
    {
        private readonly HttpService _http = new HttpService();

        #region ====== INTERNAL DTO ======

        /// <summary>
        /// Dữ liệu tin nhắn nhóm lưu trên Firebase.
        /// </summary>
        public class GroupMessageData
        {
            public string SenderId { get; set; }
            public string Content { get; set; }
            public long Timestamp { get; set; }

            /// <summary>
            /// "text" / "file" / "image".
            /// </summary>
            public string Type { get; set; }

            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string FileUrl { get; set; }

            public string ImageMimeType { get; set; }
            public string ImageBase64 { get; set; }
        }

        private class FirebasePostResult
        {
            public string name { get; set; }
        }

        #endregion

        #region ====== URL HELPERS ======

        private static string Db(string path, string token)
        {
            if (path == null) path = string.Empty;

            string auth = string.IsNullOrEmpty(token) ? string.Empty : ("?auth=" + token);
            return FirebaseConfig.DatabaseUrl.TrimEnd('/') + "/" + path + ".json" + auth;
        }

        private static string AppendQuery(string baseUrl, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return baseUrl;
            }

            return baseUrl + (baseUrl.Contains("?") ? "&" : "?") + query;
        }

        /// <summary>
        /// Quote chuỗi đúng kiểu Firebase REST query (orderBy="Timestamp").
        /// </summary>
        private static string Q(string value)
        {
            return "\"" + value + "\"";
        }

        #endregion

        #region ====== SEND TEXT / FILE ======

        /// <summary>
        /// Gửi tin nhắn text vào groupMessages/{groupId}.
        /// </summary>
        public async Task<string> SendTextAsync(
            string groupId,
            string senderLocalId,
            string text,
            long timestamp,
            string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            string sid = KeySanitizer.SafeKey(senderLocalId);

            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(sid))
            {
                throw new ArgumentException("senderLocalId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            GroupMessageData data = new GroupMessageData();
            data.SenderId = sid;
            data.Content = text.Trim();
            data.Timestamp = timestamp;
            data.Type = "text";

            FirebasePostResult res = await _http.PostAsync<FirebasePostResult>(
                Db("groupMessages/" + gid, token), data).ConfigureAwait(false);

            return res != null ? res.name : null;
        }

        /// <summary>
        /// (Giữ tương thích) Gửi text với timestamp = now.
        /// </summary>
        public Task<string> SendTextAsync(string groupId, string senderLocalId, string text, string token = null)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return SendTextAsync(groupId, senderLocalId, text, now, token);
        }

        /// <summary>
        /// Gửi tin nhắn file vào groupMessages/{groupId}.
        /// </summary>
        public async Task<string> SendFileAsync(
            string groupId,
            string senderLocalId,
            string fileName,
            long fileSize,
            string fileUrl,
            long timestamp,
            string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            string sid = KeySanitizer.SafeKey(senderLocalId);

            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(sid))
            {
                throw new ArgumentException("senderLocalId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                throw new ArgumentException("fileUrl rỗng.");
            }

            GroupMessageData data = new GroupMessageData();
            data.SenderId = sid;
            data.Content = string.Empty;
            data.Timestamp = timestamp;
            data.Type = "file";
            data.FileName = fileName ?? string.Empty;
            data.FileSize = fileSize;
            data.FileUrl = fileUrl;

            FirebasePostResult res = await _http.PostAsync<FirebasePostResult>(
                Db("groupMessages/" + gid, token), data).ConfigureAwait(false);

            return res != null ? res.name : null;
        }


        /// <summary>
        /// Gửi tin nhắn ảnh (base64) vào groupMessages/{groupId}.
        /// Lưu ý: base64 sẽ làm data lớn hơn, chỉ nên dùng cho ảnh dung lượng nhỏ/vừa.
        /// </summary>
        public async Task<string> SendImageAsync(
            string groupId,
            string senderLocalId,
            string fileName,
            long fileSize,
            string mimeType,
            string imageBase64,
            long timestamp,
            string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            string sid = KeySanitizer.SafeKey(senderLocalId);

            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(sid))
            {
                throw new ArgumentException("senderLocalId rỗng.");
            }

            if (string.IsNullOrWhiteSpace(imageBase64))
            {
                throw new ArgumentException("imageBase64 rỗng.");
            }

            GroupMessageData data = new GroupMessageData();
            data.SenderId = sid;
            data.Content = string.Empty;
            data.Timestamp = timestamp;
            data.Type = "image";
            data.FileName = fileName ?? string.Empty;
            data.FileSize = fileSize;
            data.ImageMimeType = string.IsNullOrWhiteSpace(mimeType) ? "image/*" : mimeType;
            data.ImageBase64 = imageBase64;

            FirebasePostResult res = await _http.PostAsync<FirebasePostResult>(
                Db("groupMessages/" + gid, token), data).ConfigureAwait(false);

            return res != null ? res.name : null;
        }

        #endregion

        #region ====== LOAD HISTORY ======

        /// <summary>
        /// Load toàn bộ lịch sử tin nhắn nhóm.
        /// </summary>
        public async Task<Dictionary<string, GroupMessageData>> GetAllAsync(string groupId, string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            Dictionary<string, GroupMessageData> dict =
                await _http.GetAsync<Dictionary<string, GroupMessageData>>(
                    Db("groupMessages/" + gid, token)).ConfigureAwait(false);

            return dict ?? new Dictionary<string, GroupMessageData>();
        }

        /// <summary>
        /// Load tin nhắn mới từ một mốc thời gian (Timestamp) trở đi.
        /// Lưu ý: REST query của Firebase sẽ trả cả phần tử bằng startAt,
        /// nên nên truyền sinceTimestamp+1 để tránh trùng.
        /// </summary>
        public async Task<Dictionary<string, GroupMessageData>> GetSinceAsync(
            string groupId,
            long startAtTimestamp,
            string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            // orderBy="Timestamp"&startAt=123
            string q = "orderBy=" + Uri.EscapeDataString(Q("Timestamp")) +
                       "&startAt=" + startAtTimestamp.ToString(CultureInfo.InvariantCulture);

            Dictionary<string, GroupMessageData> dict =
                await _http.GetAsync<Dictionary<string, GroupMessageData>>(
                    AppendQuery(Db("groupMessages/" + gid, token), q)).ConfigureAwait(false);

            return dict ?? new Dictionary<string, GroupMessageData>();
        }

        /// <summary>
        /// Load các tin nhắn gần nhất (limitToLast).
        /// </summary>
        public async Task<Dictionary<string, GroupMessageData>> GetRecentAsync(
            string groupId,
            int limit = 80,
            string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            if (limit <= 0) limit = 80;
            if (limit > 300) limit = 300;

            string q = "orderBy=" + Uri.EscapeDataString(Q("Timestamp")) +
                       "&limitToLast=" + limit.ToString(CultureInfo.InvariantCulture);

            Dictionary<string, GroupMessageData> dict =
                await _http.GetAsync<Dictionary<string, GroupMessageData>>(
                    AppendQuery(Db("groupMessages/" + gid, token), q)).ConfigureAwait(false);

            return dict ?? new Dictionary<string, GroupMessageData>();
        }

        #endregion
    }
}
