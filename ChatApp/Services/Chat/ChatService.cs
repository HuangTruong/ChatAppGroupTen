using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Helpers;
using ChatApp.Models.Chat;
using FireSharp.Interfaces;

namespace ChatApp.Services.Chat
{
    /// <summary>
    /// Service xử lý toàn bộ logic chat 1-1:
    /// - Tạo mã cuộc trò chuyện (CID) giữa 2 người.
    /// - Gửi tin nhắn trực tiếp.
    /// - Tải lịch sử tin nhắn.
    /// - Xoá tin nhắn, đánh dấu đã xem.
    /// </summary>
    public class ChatService
    {
        #region ======== Trường / Khởi tạo ========

        /// <summary>
        /// Client Firebase dùng để đọc/ghi dữ liệu cuộc trò chuyện.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Khởi tạo <see cref="ChatService"/> với client Firebase đã cấu hình.
        /// </summary>
        /// <param name="firebase">Client Firebase dùng cho service.</param>
        public ChatService(IFirebaseClient firebase)
        {
            if (firebase == null) throw new ArgumentNullException("firebase");
            _firebase = firebase;
        }

        #endregion

        #region ======== CID & Đường dẫn chat 1-1 ========

        /// <summary>
        /// Tạo mã cuộc trò chuyện chung giữa 2 người.
        /// Dùng tên theo thứ tự từ điển: nhỏ__lớn để đảm bảo CID cố định.
        /// </summary>
        /// <param name="u1">Tên người dùng thứ nhất.</param>
        /// <param name="u2">Tên người dùng thứ hai.</param>
        /// <returns>Chuỗi CID dạng <c>"userA__userB"</c>.</returns>
        public string BuildCid(string u1, string u2)
        {
            return string.CompareOrdinal(u1, u2) < 0
                ? u1 + "__" + u2
                : u2 + "__" + u1;
        }

        /// <summary>
        /// Lấy đường dẫn node Firebase tương ứng với cuộc trò chuyện 1-1.
        /// </summary>
        /// <param name="u1">Tên người dùng thứ nhất.</param>
        /// <param name="u2">Tên người dùng thứ hai.</param>
        /// <returns>Đường dẫn dạng <c>"cuocTroChuyen/{cid}"</c>.</returns>
        public string GetDirectChatPath(string u1, string u2)
        {
            return "cuocTroChuyen/" + BuildCid(u1, u2);
        }

        #endregion

        #region ======== Gửi tin nhắn 1-1 ========

        /// <summary>
        /// Gửi tin nhắn giữa 2 người dùng:
        /// - Push tin nhắn lên Firebase để sinh ID.
        /// - Cập nhật lại bản ghi có trường <c>id</c> đầy đủ.
        /// </summary>
        /// <param name="from">Người gửi.</param>
        /// <param name="to">Người nhận.</param>
        /// <param name="content">Nội dung tin nhắn.</param>
        /// <returns>Đối tượng <see cref="TinNhan"/> đã được gán ID.</returns>
        public async Task<TinNhan> SendDirectAsync(string from, string to, string content)
        {
            if (string.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException("from");
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException("to");

            string cid = BuildCid(from, to);
            string path = "cuocTroChuyen/" + cid + "/";

            var tn = new TinNhan
            {
                guiBoi = from,
                nhanBoi = to,
                noiDung = content ?? string.Empty,
                thoiGian = DateTime.UtcNow.ToString("o")
            };

            // Push để Firebase tự sinh id
            var push = await _firebase.PushAsync(path, tn);
            tn.id = push.Result.name;

            // Ghi lại bản đầy đủ gắn id
            await _firebase.SetAsync("cuocTroChuyen/" + cid + "/" + tn.id, tn);
            return tn;
        }

        #endregion

        #region ======== Load lịch sử chat 1-1 ========

        /// <summary>
        /// Tải toàn bộ tin nhắn giữa 2 người, sắp xếp tăng dần theo thời gian gửi.
        /// </summary>
        /// <param name="a">Người dùng A.</param>
        /// <param name="b">Người dùng B.</param>
        /// <returns>
        /// Danh sách <see cref="TinNhan"/> đã được chuẩn hóa:
        /// luôn có <c>id</c>, <c>noiDung</c> và <c>thoiGian</c> hợp lệ.
        /// </returns>
        public async Task<List<TinNhan>> LoadDirectAsync(string a, string b)
        {
            string cid = BuildCid(a, b);
            var res = await _firebase.GetAsync("cuocTroChuyen/" + cid);
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (data == null || data.Count == 0)
                return new List<TinNhan>();

            var list = data
                .Select(delegate (KeyValuePair<string, TinNhan> kv)
                {
                    var t = kv.Value ?? new TinNhan();

                    if (string.IsNullOrEmpty(t.id))
                        t.id = kv.Key;

                    if (t.noiDung == null)
                        t.noiDung = string.Empty;

                    if (string.IsNullOrEmpty(t.thoiGian))
                        t.thoiGian = DateTime.UtcNow.ToString("o");

                    return t;
                })
                .OrderBy(delegate (TinNhan t)
                {
                    return TimeParser.ToUtc(t.thoiGian);
                })
                .ToList();

            return list;
        }

        #endregion

        #region ======== Xoá tin nhắn / Đánh dấu đã xem ========

        /// <summary>
        /// Xoá một tin nhắn theo ID trong cuộc trò chuyện 1-1 giữa 2 người.
        /// </summary>
        /// <param name="a">Người dùng A.</param>
        /// <param name="b">Người dùng B.</param>
        /// <param name="msgId">ID tin nhắn cần xoá.</param>
        public async Task DeleteAsync(string a, string b, string msgId)
        {
            if (string.IsNullOrEmpty(msgId))
                return;

            string cid = BuildCid(a, b);
            await _firebase.DeleteAsync("cuocTroChuyen/" + cid + "/" + msgId);
        }

        /// <summary>
        /// Đánh dấu một tin nhắn là đã xem bởi người dùng <paramref name="self"/>:
        /// lưu thời điểm xem (Unix ms) vào node <c>reads/{self}</c> của tin nhắn.
        /// </summary>
        /// <param name="self">Người đang xem (tài khoản hiện tại).</param>
        /// <param name="other">Người trò chuyện còn lại.</param>
        /// <param name="msgId">ID tin nhắn được đánh dấu đã xem.</param>
        public async Task MarkLastSeenAsync(string self, string other, string msgId)
        {
            if (string.IsNullOrEmpty(msgId))
                return;

            string cid = BuildCid(self, other);
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await _firebase.SetAsync(
                "cuocTroChuyen/" + cid + "/" + msgId + "/reads/" + self,
                now);
        }

        #endregion
    }
}
