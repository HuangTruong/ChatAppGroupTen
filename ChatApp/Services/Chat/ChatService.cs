using ChatApp.Models.Chat;
using ChatApp.Helpers;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Chat
{
    public class ChatService
    {
        private readonly IFirebaseClient _firebase; // Kết nối Firebase

        public ChatService(IFirebaseClient firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Tạo mã cuộc trò chuyện chung giữa 2 người
        public string BuildCid(string u1, string u2)
        {
            return string.CompareOrdinal(u1, u2) < 0
                ? $"{u1}__{u2}"
                : $"{u2}__{u1}";
        }

        // Đường dẫn Firebase cho cuộc trò chuyện 1-1
        public string GetDirectChatPath(string u1, string u2)
        {
            return $"cuocTroChuyen/{BuildCid(u1, u2)}";
        }

        // Gửi tin nhắn giữa 2 người
        public async Task<TinNhan> SendDirectAsync(string from, string to, string content)
        {
            if (string.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var cid = BuildCid(from, to);
            string path = $"cuocTroChuyen/{cid}/";

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
            await _firebase.SetAsync($"cuocTroChuyen/{cid}/{tn.id}", tn);
            return tn;
        }

        // Tải toàn bộ tin nhắn giữa 2 người (đã sort theo thời gian)
        public async Task<List<TinNhan>> LoadDirectAsync(string a, string b)
        {
            var cid = BuildCid(a, b);
            var res = await _firebase.GetAsync($"cuocTroChuyen/{cid}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (data == null || data.Count == 0)
                return new List<TinNhan>();

            var list = data
                .Select(kv =>
                {
                    var t = kv.Value ?? new TinNhan();
                    if (string.IsNullOrEmpty(t.id)) t.id = kv.Key;
                    if (t.noiDung == null) t.noiDung = string.Empty;
                    if (string.IsNullOrEmpty(t.thoiGian))
                        t.thoiGian = DateTime.UtcNow.ToString("o");
                    return t;
                })
                .OrderBy(t => TimeParser.ToUtc(t.thoiGian))
                .ToList();

            return list;
        }

        // Xoá tin nhắn theo ID
        public async Task DeleteAsync(string a, string b, string msgId)
        {
            if (string.IsNullOrEmpty(msgId)) return;

            var cid = BuildCid(a, b);
            await _firebase.DeleteAsync($"cuocTroChuyen/{cid}/{msgId}");
        }

        // Đánh dấu tin nhắn đã xem
        public async Task MarkLastSeenAsync(string self, string other, string msgId)
        {
            if (string.IsNullOrEmpty(msgId)) return;

            var cid = BuildCid(self, other);
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _firebase.SetAsync($"cuocTroChuyen/{cid}/{msgId}/reads/{self}", now);
        }
    }
}
