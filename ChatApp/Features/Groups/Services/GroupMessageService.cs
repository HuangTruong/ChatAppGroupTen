using ChatApp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// GroupMessageService: Service làm việc với tin nhắn nhóm trên Firebase (REST).
    ///
    /// Bạn cần nhớ 1 node chính:
    /// - groupMessages/{groupId}/{messageId}
    ///
    /// Service này làm 2 việc:
    /// 1) Gửi tin nhắn (text / file / image)
    /// 2) Tải lịch sử (tất cả / từ mốc thời gian / gần nhất)
    /// </summary>
    public class GroupMessageService
    {
        #region ====== BIẾN THÀNH VIÊN ======

        private readonly HttpService _http = new HttpService();

        #endregion

        #region ====== DTO NỘI BỘ (DỮ LIỆU LƯU TRÊN FIREBASE) ======

        /// <summary>
        /// Dữ liệu tin nhắn nhóm lưu trên Firebase.
        /// (Đây là schema JSON của 1 message trong groupMessages/{groupId}/{messageId})
        /// </summary>
        public class GroupMessageData
        {
            public string SenderId { get; set; }
            public string Content { get; set; }
            public long Timestamp { get; set; }

            /// <summary>
            /// Loại tin: "text" / "file" / "image"
            /// </summary>
            public string Type { get; set; }

            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string FileUrl { get; set; }

            public string ImageMimeType { get; set; }
            public string ImageBase64 { get; set; }
        }

        /// <summary>
        /// Kết quả trả về khi POST lên Firebase:
        /// Firebase sẽ trả { "name": "pushKey..." }
        /// </summary>
        private class FirebasePostResult
        {
            public string name { get; set; }
        }

        #endregion

        #region ====== HỖ TRỢ TẠO URL FIREBASE ======

        private static string Db(string path, string token)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            string authQuery;
            if (string.IsNullOrEmpty(token))
            {
                authQuery = string.Empty;
            }
            else
            {
                authQuery = "?auth=" + token;
            }

            return FirebaseConfig.DatabaseUrl.TrimEnd('/') + "/" + path + ".json" + authQuery;
        }

        private static string AppendQuery(string baseUrl, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return baseUrl;
            }

            bool hasQuestion = baseUrl.Contains("?");
            if (hasQuestion)
            {
                return baseUrl + "&" + query;
            }

            return baseUrl + "?" + query;
        }

        /// <summary>
        /// Bọc chuỗi theo format query của Firebase REST:
        /// ví dụ orderBy="Timestamp"
        /// </summary>
        private static string QuoteForFirebase(string value)
        {
            return "\"" + value + "\"";
        }

        #endregion

        #region ====== GỬI TIN NHẮN NHÓM ======

        /// <summary>
        /// Gửi tin nhắn text vào groupMessages/{groupId}.
        /// Trả về messageId (push key) nếu thành công.
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

            FirebasePostResult res = await _http
                .PostAsync<FirebasePostResult>(Db("groupMessages/" + gid, token), data)
                .ConfigureAwait(false);

            if (res == null)
            {
                return null;
            }

            return res.name;
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
        /// Trả về messageId (push key) nếu thành công.
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
            data.FileName = fileName == null ? string.Empty : fileName;
            data.FileSize = fileSize;
            data.FileUrl = fileUrl;

            FirebasePostResult res = await _http
                .PostAsync<FirebasePostResult>(Db("groupMessages/" + gid, token), data)
                .ConfigureAwait(false);

            if (res == null)
            {
                return null;
            }

            return res.name;
        }

        /// <summary>
        /// Gửi tin nhắn ảnh (base64) vào groupMessages/{groupId}.
        /// Lưu ý: base64 làm JSON lớn hơn, chỉ nên dùng cho ảnh nhỏ/vừa.
        /// Trả về messageId (push key) nếu thành công.
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
            data.FileName = fileName == null ? string.Empty : fileName;
            data.FileSize = fileSize;
            data.ImageMimeType = string.IsNullOrWhiteSpace(mimeType) ? "image/*" : mimeType;
            data.ImageBase64 = imageBase64;

            FirebasePostResult res = await _http
                .PostAsync<FirebasePostResult>(Db("groupMessages/" + gid, token), data)
                .ConfigureAwait(false);

            if (res == null)
            {
                return null;
            }

            return res.name;
        }

        #endregion

        #region ====== TẢI LỊCH SỬ TIN NHẮN NHÓM ======

        /// <summary>
        /// Tải toàn bộ lịch sử tin nhắn nhóm.
        /// Trả về Dictionary(messageId -> data).
        /// </summary>
        public async Task<Dictionary<string, GroupMessageData>> GetAllAsync(string groupId, string token = null)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ArgumentException("groupId rỗng.");
            }

            Dictionary<string, GroupMessageData> dict = await _http
                .GetAsync<Dictionary<string, GroupMessageData>>(Db("groupMessages/" + gid, token))
                .ConfigureAwait(false);

            if (dict == null)
            {
                return new Dictionary<string, GroupMessageData>();
            }

            return dict;
        }

        /// <summary>
        /// Tải các tin nhắn từ mốc thời gian (Timestamp) trở đi.
        /// Lưu ý: Firebase REST trả cả phần tử bằng startAt,
        /// nên thường truyền sinceTimestamp + 1 để tránh trùng.
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

            string query = "orderBy=" + Uri.EscapeDataString(QuoteForFirebase("Timestamp")) +
                           "&startAt=" + startAtTimestamp.ToString(CultureInfo.InvariantCulture);

            string url = AppendQuery(Db("groupMessages/" + gid, token), query);

            Dictionary<string, GroupMessageData> dict = await _http
                .GetAsync<Dictionary<string, GroupMessageData>>(url)
                .ConfigureAwait(false);

            if (dict == null)
            {
                return new Dictionary<string, GroupMessageData>();
            }

            return dict;
        }

        /// <summary>
        /// Tải các tin nhắn gần nhất (limitToLast).
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

            if (limit <= 0)
            {
                limit = 80;
            }
            if (limit > 300)
            {
                limit = 300;
            }

            string query = "orderBy=" + Uri.EscapeDataString(QuoteForFirebase("Timestamp")) +
                           "&limitToLast=" + limit.ToString(CultureInfo.InvariantCulture);

            string url = AppendQuery(Db("groupMessages/" + gid, token), query);

            Dictionary<string, GroupMessageData> dict = await _http
                .GetAsync<Dictionary<string, GroupMessageData>>(url)
                .ConfigureAwait(false);

            if (dict == null)
            {
                return new Dictionary<string, GroupMessageData>();
            }

            return dict;
        }

        #endregion
    }
}
