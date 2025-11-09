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
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase)); // Kiểm tra null
        }

        // Tạo mã cuộc trò chuyện chung giữa 2 người
        public string BuildCid(string u1, string u2)
        {
            return string.CompareOrdinal(u1, u2) < 0
                ? $"{u1}__{u2}"
                : $"{u2}__{u1}";
        }

        // Gửi tin nhắn giữa 2 người
        public async Task<TinNhan> SendDirectAsync(string from, string to, string content)
        {
            var cid = BuildCid(from, to);

            var tn = new TinNhan
            {
                guiBoi = from,
                nhanBoi = to,
                noiDung = content ?? string.Empty,
                thoiGian = DateTime.UtcNow.ToString("o")
            };

            var push = await _firebase.PushAsync($"cuocTroChuyen/{cid}/", tn); // Đẩy tin nhắn mới
            tn.id = push.Result.name;

            await _firebase.SetAsync($"cuocTroChuyen/{cid}/{tn.id}", tn); // Ghi tin nhắn đầy đủ
            return tn;
        }

        // Tải danh sách tin nhắn giữa 2 người
        public async Task<List<TinNhan>> LoadDirectAsync(string a, string b)
        {
            var cid = BuildCid(a, b);
            var res = await _firebase.GetAsync($"cuocTroChuyen/{cid}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (data == null) return new List<TinNhan>();

            return data
                .Select(kv =>
                {
                    var t = kv.Value ?? new TinNhan();
                    if (string.IsNullOrEmpty(t.id)) t.id = kv.Key;
                    if (t.noiDung == null) t.noiDung = string.Empty;
                    return t;
                })
                .OrderBy(t => TimeParser.ToUtc(t.thoiGian)) // Sắp theo thời gian
                .ToList();
        }

        // Xoá tin nhắn theo ID
        public async Task DeleteAsync(string a, string b, string msgId)
        {
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
