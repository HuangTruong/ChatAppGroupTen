using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Lắng nghe realtime tin nhắn qua Firebase Streaming (SSE).
    /// </summary>
    public class RealtimeMessageListener : IDisposable
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly IFirebaseClient _client;
        private EventStreamResponse _stream;

        public event Action<string, object> OnMessageAdded;

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public RealtimeMessageListener(IFirebaseClient client)
        {
            _client = client;
        }

        #endregion

        #region ====== BẮT ĐẦU / DỪNG LẮNG NGHE ======

        /// <summary>
        /// Bắt đầu lắng nghe path messages/{conversationId}
        /// </summary>
        public async Task StartAsync(string path)
        {
            Stop();

            _stream = await _client.OnAsync(
                path,
                added: (s, args, context) =>
                {
                    if (args.Data == "null") return;

                    try
                    {
                        var msg = JsonConvert.DeserializeObject<object>(args.Data);
                        OnMessageAdded?.Invoke(args.Path, msg);
                    }
                    catch
                    {
                        // ignore parse lỗi
                    }
                },
                changed: null,
                removed: null
            );
        }

        public void Stop()
        {
            try { _stream?.Dispose(); } catch { }
            _stream = null;
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
