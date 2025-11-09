using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FireSharp.Interfaces;

namespace ChatApp.Services.Chat
{
    public class TypingService
    {
        private readonly IFirebaseClient _firebase; // Kết nối Firebase

        public TypingService(IFirebaseClient firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Gửi tín hiệu "đang nhập" cho người khác thấy
        public async Task SendTypingAsync(string key, string username, int seconds = 4)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(username))
                return;

            long until = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds(); // Thời điểm hết hiệu lực

            await _firebase.SetAsync($"typing/{key}/{username}", new { until });
        }

        // Kiểm tra có ai đang nhập không (ngoại trừ mình)
        public async Task<(bool CoNguoiNhap, string TenNguoiNhap)> GetTypingStateAsync(string key, string currentUser)
        {
            if (string.IsNullOrEmpty(key))
                return (false, string.Empty);

            var res = await _firebase.GetAsync("typing/" + key);
            var data = res.ResultAs<Dictionary<string, Dictionary<string, long>>>();

            if (data == null) return (false, string.Empty);

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var kv in data)
            {
                var ten = kv.Key;
                if (ten.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                    continue; // Bỏ qua chính mình

                if (kv.Value != null &&
                    kv.Value.TryGetValue("until", out long until) &&
                    until > now)
                {
                    return (true, ten); // Có người đang nhập
                }
            }

            return (false, string.Empty); // Không ai đang nhập
        }
    }
}
