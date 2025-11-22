using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FireSharp.Interfaces;

namespace ChatApp.Services.Chat
{
    #region TypingService
    /// <summary>
    /// Service quản lý trạng thái "đang nhập" (typing indicator) trong cuộc trò chuyện.
    /// Lưu trữ dữ liệu tại node:
    /// typing/{key}/{username} → { until: <unix-ms-expire> }
    /// </summary>
    public class TypingService
    {
        /// <summary>
        /// Client Firebase dùng để ghi/đọc trạng thái typing.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Khởi tạo <see cref="TypingService"/> với client Firebase.
        /// </summary>
        /// <param name="firebase">Client Firebase đã cấu hình.</param>
        /// <exception cref="ArgumentNullException">
        /// Ném ra nếu <paramref name="firebase"/> là <c>null</c>.
        /// </exception>
        public TypingService(IFirebaseClient firebase)
        {
            if (firebase == null)
                throw new ArgumentNullException("firebase");

            _firebase = firebase;
        }

        #region SendTypingAsync
        /// <summary>
        /// Gửi tín hiệu "đang nhập" đến Firebase để người khác thấy.
        /// </summary>
        /// <param name="key">
        /// Khóa cuộc trò chuyện (ví dụ: cid hoặc groupId).
        /// </param>
        /// <param name="username">
        /// Username người đang nhập.
        /// </param>
        /// <param name="seconds">
        /// Số giây hiệu lực của tín hiệu. Mặc định: 4 giây.
        /// </param>
        /// <remarks>
        /// Tín hiệu sẽ tự hết hạn nhờ timestamp &lt;until&gt; (UTC + seconds).
        /// </remarks>
        public async Task SendTypingAsync(string key, string username, int seconds = 4)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(username))
                return;

            long until =
                DateTimeOffset.UtcNow.AddSeconds(seconds)
                .ToUnixTimeMilliseconds();

            await _firebase.SetAsync(
                "typing/" + key + "/" + username,
                new { until = until }
            );
        }
        #endregion

        #region GetTypingStateAsync
        /// <summary>
        /// Kiểm tra xem có ai (ngoại trừ người hiện tại) đang nhập trong cuộc trò chuyện.
        /// </summary>
        /// <param name="key">Khóa cuộc trò chuyện.</param>
        /// <param name="currentUser">Tên người dùng hiện tại.</param>
        /// <returns>
        /// Tuple gồm:
        /// - <c>CoNguoiNhap</c>: true nếu có ai đang nhập.
        /// - <c>TenNguoiNhap</c>: tên người đang nhập (nếu có).
        /// </returns>
        public async Task<(bool CoNguoiNhap, string TenNguoiNhap)> GetTypingStateAsync(
            string key,
            string currentUser)
        {
            if (string.IsNullOrEmpty(key))
                return (false, string.Empty);

            var res = await _firebase.GetAsync("typing/" + key);

            // typing/{key}/{username}/until = long
            var data =
                res.ResultAs<Dictionary<string, Dictionary<string, long>>>();

            if (data == null)
                return (false, string.Empty);

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var kv in data)
            {
                string ten = kv.Key;

                if (string.Equals(ten, currentUser, StringComparison.OrdinalIgnoreCase))
                    continue; // bỏ qua chính mình

                Dictionary<string, long> fields = kv.Value;
                if (fields == null)
                    continue;

                long until;
                if (fields.TryGetValue("until", out until) && until > now)
                {
                    return (true, ten); // có người đang nhập
                }
            }

            return (false, string.Empty);
        }
        #endregion
    }
    #endregion
}
