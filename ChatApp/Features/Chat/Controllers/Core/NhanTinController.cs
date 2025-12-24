using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Models.Messages;
using ChatApp.Services.FileHost;
using ChatApp.Services.Attachments;

using FireSharp;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FirebaseAppConfig = ChatApp.Services.Firebase.FirebaseConfig;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý nhắn tin 1-1:
    /// - Load lịch sử hội thoại
    /// - Listen realtime (incremental + debounce reload full)
    /// - Gửi text / ảnh / file
    /// </summary>
    public class NhanTinController : IDisposable
    {
        #region ====== BIẾN THÀNH VIÊN (FIELDS) ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string _currentUserId;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string _token;

        /// <summary>
        /// Firebase client (FireSharp).
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Stream realtime đang lắng nghe.
        /// </summary>
        private EventStreamResponse _stream;

        /// <summary>
        /// ConversationId hiện tại đang lắng nghe (để chặn event cũ).
        /// </summary>
        private string _listeningConversationId;

        // Track message đã biết để tránh duplicate khi stream "added"
        private readonly object _knownLock = new object();
        private readonly HashSet<string> _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

        // Debounce reload full chỉ khi thật sự cần (changed/remove/snapshot)
        private readonly object _reloadLock = new object();
        private Timer _reloadDebounceTimer;

        private bool _ignoreFirstSnapshot;
        private Action<List<ChatMessage>> _onReset;


        #endregion

        #region ====== KHỞI TẠO (CTOR) ======

        /// <summary>
        /// Khởi tạo controller nhắn tin.
        /// </summary>
        /// <param name="currentUserId">localId user hiện tại.</param>
        /// <param name="token">token đăng nhập (nếu cần về sau).</param>
        public NhanTinController(string currentUserId, string token)
        {
            _currentUserId = currentUserId;
            _token = token;

            IFirebaseConfig cfg = new FirebaseConfig();
            cfg.BasePath = FirebaseAppConfig.DatabaseUrl;
            cfg.AuthSecret = string.Empty;

            _firebase = new FirebaseClient(cfg);
        }

        #endregion

        #region ====== TIỆN ÍCH NỘI BỘ (HELPERS) ======

        /// <summary>
        /// Tạo conversationId ổn định cho 2 user (theo thứ tự từ điển).
        /// </summary>
        private string BuildConversationId(string userId1, string userId2)
        {
            int cmp = string.CompareOrdinal(userId1, userId2);
            if (cmp < 0) return userId1 + "_" + userId2;
            return userId2 + "_" + userId1;
        }

        /// <summary>
        /// So sánh 2 tin nhắn theo Timestamp tăng dần.
        /// </summary>
        private static int CompareByTime(ChatMessage a, ChatMessage b)
        {
            long ta = (a != null) ? a.Timestamp : 0;
            long tb = (b != null) ? b.Timestamp : 0;

            if (ta < tb) return -1;
            if (ta > tb) return 1;
            return 0;
        }

        /// <summary>
        /// Parse 1 ChatMessage từ JSON (fail => null).
        /// </summary>
        private ChatMessage DeserializeMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try { return JsonConvert.DeserializeObject<ChatMessage>(json); }
            catch { return null; }
        }

        /// <summary>
        /// Parse map {messageId -> ChatMessage} từ JSON (fail => null).
        /// </summary>
        private Dictionary<string, ChatMessage> DeserializeMessageMap(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try { return JsonConvert.DeserializeObject<Dictionary<string, ChatMessage>>(json); }
            catch { return null; }
        }

        /// <summary>
        /// Áp các giá trị chuẩn cho message:
        /// - MessageId, IsMine
        /// - Default MessageType = "text" nếu trống
        /// </summary>
        private void NormalizeMessage(ChatMessage m, string messageId)
        {
            if (m == null) return;

            m.MessageId = messageId;
            m.IsMine = string.Equals(m.SenderId, _currentUserId, StringComparison.Ordinal);

            if (string.IsNullOrWhiteSpace(m.MessageType))
            {
                m.MessageType = "text";
            }
        }

        /// <summary>
        /// Reset danh sách known messageIds theo list hiện có.
        /// </summary>
        private void ResetKnownIds(List<ChatMessage> list)
        {
            lock (_knownLock)
            {
                _knownMessageIds.Clear();
                if (list == null) return;

                for (int i = 0; i < list.Count; i++)
                {
                    ChatMessage m = list[i];
                    if (m == null) continue;

                    if (!string.IsNullOrEmpty(m.MessageId))
                    {
                        _knownMessageIds.Add(m.MessageId);
                    }
                }
            }
        }

        /// <summary>
        /// Đánh dấu messageId là đã thấy; trả về true nếu là mới.
        /// </summary>
        private bool MarkKnownIfNew(string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return false;

            lock (_knownLock)
            {
                if (_knownMessageIds.Contains(messageId)) return false;
                _knownMessageIds.Add(messageId);
                return true;
            }
        }

        #endregion

        #region ====== HỘI THOẠI (LOAD) ======

        /// <summary>
        /// Lấy toàn bộ hội thoại giữa user hiện tại và user khác.
        /// </summary>
        public async Task<List<ChatMessage>> GetConversationAsync(string otherUserId)
        {
            string conversationId = BuildConversationId(_currentUserId, otherUserId);
            return await LoadConversationAsync(conversationId);
        }

        /// <summary>
        /// Load full hội thoại theo conversationId.
        /// </summary>
        private async Task<List<ChatMessage>> LoadConversationAsync(string conversationId)
        {
            string path = "messages/" + conversationId;

            FirebaseResponse resp = await _firebase.GetAsync(path);
            Dictionary<string, ChatMessage> raw = resp.ResultAs<Dictionary<string, ChatMessage>>();

            List<ChatMessage> list = new List<ChatMessage>();

            if (raw != null)
            {
                foreach (KeyValuePair<string, ChatMessage> kv in raw)
                {
                    ChatMessage m = kv.Value;
                    if (m == null) continue;

                    NormalizeMessage(m, kv.Key);
                    list.Add(m);
                }

                list.Sort(CompareByTime);
            }

            return list;
        }

        #endregion

        #region ====== NGƯỜI DÙNG & BẠN BÈ ======

        /// <summary>
        /// Lấy toàn bộ users trong node "users".
        /// </summary>
        public async Task<Dictionary<string, User>> GetAllUsersAsync()
        {
            FirebaseResponse resp = await _firebase.GetAsync("users");
            Dictionary<string, User> data = resp.ResultAs<Dictionary<string, User>>();

            if (data == null) return new Dictionary<string, User>();

            foreach (KeyValuePair<string, User> kv in data)
            {
                if (kv.Value != null)
                {
                    kv.Value.LocalId = kv.Key;
                }
            }

            return data;
        }

        /// <summary>
        /// Lấy danh sách bạn của currentLocalId (đã join sang node users).
        /// </summary>
        public async Task<Dictionary<string, User>> GetFriendUsersAsync(string currentLocalId)
        {
            string safeMe = KeySanitizer.SafeKey(currentLocalId);

            FirebaseResponse respFriends = await _firebase.GetAsync("friends/" + safeMe);
            Dictionary<string, bool> friendIds = respFriends.ResultAs<Dictionary<string, bool>>();

            if (friendIds == null || friendIds.Count == 0)
            {
                return new Dictionary<string, User>();
            }

            // Nhiều bạn => join 1 lần bằng allUsers để giảm số request.
            if (friendIds.Count > 50)
            {
                Dictionary<string, User> allUsers = await GetAllUsersAsync();
                Dictionary<string, User> joined = new Dictionary<string, User>();

                foreach (KeyValuePair<string, bool> kv in friendIds)
                {
                    string friendId = kv.Key;
                    User u;

                    if (allUsers.TryGetValue(friendId, out u) && u != null)
                    {
                        u.LocalId = friendId;
                        joined[friendId] = u;
                    }
                }

                return joined;
            }

            // Ít bạn => fetch song song có giới hạn.
            Dictionary<string, User> result = new Dictionary<string, User>();
            object sync = new object();

            SemaphoreSlim sem = new SemaphoreSlim(6);
            List<Task> tasks = new List<Task>();

            foreach (KeyValuePair<string, bool> kv in friendIds)
            {
                string friendId = kv.Key;

                tasks.Add(Task.Run(async () =>
                {
                    await sem.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        FirebaseResponse respUser = await _firebase.GetAsync("users/" + friendId).ConfigureAwait(false);
                        User u = respUser.ResultAs<User>();

                        if (u != null)
                        {
                            u.LocalId = friendId;
                            lock (sync)
                            {
                                result[friendId] = u;
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                    finally
                    {
                        try { sem.Release(); } catch { }
                    }
                }));
            }

            try { await Task.WhenAll(tasks).ConfigureAwait(false); } catch { }

            return result;
        }

        #endregion

        #region ====== REALTIME (LISTEN INCREMENTAL) ======

        /// <summary>
        /// Listen incremental:
        /// - Load initial 1 lần
        /// - Stream "added": append tin mới
        /// - Nếu snapshot/changed/remove => debounce reload full
        /// </summary>

        /// <summary>
        /// Listen realtime đơn giản:
        /// - Load initial 1 lần
        /// - Chỉ xử lý event "added" (tin mới) => UI append mượt
        /// Lưu ý: edit/delete message sẽ không tự update (đổi lại code đơn giản + mượt).
        /// </summary>
        public async void StartListenConversation(
            string otherUserId,
            Action<List<ChatMessage>> onInitialLoaded,
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            StopListen();

            if (onInitialLoaded == null || onMessageAdded == null) return;

            string conversationId = BuildConversationId(_currentUserId, otherUserId);
            _listeningConversationId = conversationId;


            _onReset = onReset;
            _ignoreFirstSnapshot = true;
            // 1) Load initial 1 lần
            List<ChatMessage> initial;
            try { initial = await LoadConversationAsync(conversationId).ConfigureAwait(false); }
            catch { initial = new List<ChatMessage>(); }

            ResetKnownIds(initial);

            try { onInitialLoaded(initial); } catch { }

            // 2) Stream: chỉ quan tâm "added"
            string path = "messages/" + conversationId;

            try
            {
                _stream = await _firebase.OnAsync(
                    path,
                    added: (s, e, c) => { OnAddedEvent(conversationId, e, onMessageAdded); },
                    changed: (s, e, c) => { OnChangedEvent(conversationId, e); },
                    removed: (s, e, c) => { OnRemovedEvent(conversationId, e); }
                ).ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }




        /// <summary>
        /// Xử lý event stream theo 3 loại:
        /// - Snapshot "/" (có thể map đầy đủ hoặc null)
        /// - Event theo messageId ("/-Nxxx")
        /// - Event sâu hơn ("/-Nxxx/field") => debounce reload
        /// </summary>

        /// <summary>
        /// Debounce reload full hội thoại:
        /// - gom nhiều changed/remove liên tiếp thành 1 lần reload.
        /// </summary>

        /// <summary>
        /// Dừng stream, hủy timer/cts, clear knownIds.
        /// </summary>

        /// <summary>
        /// Chỉ nhận tin mới (added) và chống duplicate bằng knownIds.
        /// </summary>
        /// <summary>
        /// Nhận event "added" từ stream.
        /// - "/" (snapshot)    : ignore lần đầu (vì đã load initial), lần sau reset.
        /// - "/-N..." (message) : deserialize và append nếu chưa seen.
        /// - "/-N.../field"    : schedule reload full (debounce) để không miss patch.
        /// </summary>
        private void OnAddedEvent(string conversationId, ValueAddedEventArgs e, Action<ChatMessage> onMessageAdded)
        {
            try
            {
                if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;
                if (e == null) return;

                string p = e.Path ?? string.Empty;
                string d = e.Data ?? string.Empty;

                // Snapshot toàn node
                if (p == "/")
                {
                    if (_ignoreFirstSnapshot)
                    {
                        _ignoreFirstSnapshot = false;
                        return;
                    }

                    HandleSnapshotReset(conversationId, d);
                    return;
                }

                string trimmed = p.Trim('/');
                if (string.IsNullOrEmpty(trimmed)) return;

                // Patch sâu: /messageId/field
                if (trimmed.IndexOf('/') >= 0)
                {
                    ScheduleReload(conversationId);
                    return;
                }

                if (string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                {
                    ScheduleReload(conversationId);
                    return;
                }

                ChatMessage m = DeserializeMessage(d);
                if (m == null)
                {
                    ScheduleReload(conversationId);
                    return;
                }

                NormalizeMessage(m, trimmed);

                if (MarkKnownIfNew(trimmed))
                {
                    if (onMessageAdded != null) { try { onMessageAdded(m); } catch { } }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void OnChangedEvent(string conversationId, ValueChangedEventArgs e)
        {
            try
            {
                if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;
                if (e == null) return;

                string p = e.Path ?? string.Empty;
                string d = e.Data ?? string.Empty;

                if (p == "/")
                {
                    if (_ignoreFirstSnapshot)
                    {
                        _ignoreFirstSnapshot = false;
                        return;
                    }

                    HandleSnapshotReset(conversationId, d);
                    return;
                }

                ScheduleReload(conversationId);
            }
            catch
            {
                // ignore
            }
        }

        private void OnRemovedEvent(string conversationId, ValueRemovedEventArgs e)
        {
            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;
            ScheduleReload(conversationId);
        }

        private void HandleSnapshotReset(string conversationId, string json)
        {
            if (_onReset == null) return;
            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;

            if (string.IsNullOrWhiteSpace(json) || string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
            {
                List<ChatMessage> empty = new List<ChatMessage>();
                ResetKnownIds(empty);
                try { _onReset(empty); } catch { }
                return;
            }

            Dictionary<string, ChatMessage> map = DeserializeMessageMap(json);
            if (map == null)
            {
                ScheduleReload(conversationId);
                return;
            }

            List<ChatMessage> list = new List<ChatMessage>();
            foreach (KeyValuePair<string, ChatMessage> kv in map)
            {
                if (kv.Value == null) continue;
                NormalizeMessage(kv.Value, kv.Key);
                list.Add(kv.Value);
            }

            list.Sort(CompareByTime);
            ResetKnownIds(list);
            try { _onReset(list); } catch { }
        }

        private void ScheduleReload(string conversationId)
        {
            if (_onReset == null) return;
            if (string.IsNullOrEmpty(conversationId)) return;
            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;

            lock (_reloadLock)
            {
                if (_reloadDebounceTimer == null)
                {
                    _reloadDebounceTimer = new Timer(ReloadTimerTick, null, Timeout.Infinite, Timeout.Infinite);
                }

                _reloadDebounceTimer.Change(250, Timeout.Infinite);
            }
        }

        private async void ReloadTimerTick(object state)
        {
            string cid = _listeningConversationId;
            Action<List<ChatMessage>> cb = _onReset;

            if (string.IsNullOrEmpty(cid) || cb == null) return;

            List<ChatMessage> full;
            try { full = await LoadConversationAsync(cid).ConfigureAwait(false); }
            catch { full = new List<ChatMessage>(); }

            ResetKnownIds(full);
            try { cb(full); } catch { }
        }

        public void StopListen()
        {
            _listeningConversationId = null;
            _onReset = null;
            _ignoreFirstSnapshot = false;

            lock (_knownLock)
            {
                _knownMessageIds.Clear();
            }

            lock (_reloadLock)
            {
                if (_reloadDebounceTimer != null)
                {
                    try { _reloadDebounceTimer.Dispose(); } catch { }
                    _reloadDebounceTimer = null;
                }
            }

            if (_stream != null)
            {
                try { _stream.Dispose(); } catch { }
                _stream = null;
            }
        }

        #endregion

        #region ====== GỬI TIN NHẮN (TEXT) ======

        /// <summary>
        /// Gửi tin nhắn text.
        /// </summary>
        public async Task SendMessageAsync(string toUserId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string conversationId = BuildConversationId(_currentUserId, toUserId);
            string path = "messages/" + conversationId;

            ChatMessage msg = new ChatMessage();
            msg.SenderId = _currentUserId;
            msg.ReceiverId = toUserId;
            msg.Text = text;
            msg.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            msg.IsMine = true;
            msg.MessageType = "text";

            await _firebase.PushAsync(path, msg);
        }

        #endregion

        #region ====== GỬI ĐÍNH KÈM (ẢNH/FILE) ======

        /// <summary>
        /// Gửi file/ảnh chung một luồng:
        /// - Nếu là ảnh: không upload Catbox, gửi trực tiếp base64 vào chat.
        /// - Nếu là file thường: upload Catbox và gửi URL.
        /// </summary>
        public async Task SendAttachmentMessageAsync(string toUserId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
            {
                throw new Exception("Chưa chọn người để chat.");
            }

            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                throw new Exception("File không tồn tại.");
            }

            bool laAnh = AttachmentClassifier.IsImageFile(filePath, out string mime);

            string conversationId = BuildConversationId(_currentUserId, toUserId);
            string path = "messages/" + conversationId;

            ChatMessage msg = new ChatMessage();
            msg.SenderId = _currentUserId;
            msg.ReceiverId = toUserId;
            msg.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            msg.IsMine = true;

            if (laAnh)
            {
                byte[] bytes = File.ReadAllBytes(filePath);

                msg.MessageType = "image";
                msg.FileName = fi.Name;
                msg.FileSize = fi.Length;
                msg.ImageMimeType = string.IsNullOrWhiteSpace(mime)
                    ? AttachmentClassifier.GetMimeTypeByExtension(filePath)
                    : mime;
                msg.ImageBase64 = Convert.ToBase64String(bytes);
            }
            else
            {
                FileAttachmentUploader uploader = new FileAttachmentUploader();
                string urlTai = await uploader.UploadAsync(filePath).ConfigureAwait(false);

                msg.MessageType = "file";
                msg.FileName = fi.Name;
                msg.FileSize = fi.Length;
                msg.FileUrl = urlTai;
            }

            await _firebase.PushAsync(path, msg).ConfigureAwait(false);
        }

        /// <summary>
        /// Giữ tương thích chỗ cũ đang gọi SendFileMessageAsync.
        /// </summary>
        public Task SendFileMessageAsync(string toUserId, string filePath)
        {
            return SendAttachmentMessageAsync(toUserId, filePath);
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN (DISPOSE) ======

        /// <summary>
        /// Dừng listen và giải phóng stream/timer.
        /// </summary>
        public void Dispose()
        {
            StopListen();
        }

        #endregion
    }
}