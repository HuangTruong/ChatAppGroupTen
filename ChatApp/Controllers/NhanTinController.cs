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
    public class NhanTinController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly string _currentUserId;
        private readonly string _token;

        private readonly IFirebaseClient _firebase;

        private EventStreamResponse _stream;
        private string _listeningConversationId;

        // Track message đã biết để tránh duplicate
        private readonly object _knownLock = new object();
        private readonly HashSet<string> _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

        // Debounce reload full chỉ khi thật sự cần (changed/remove/snapshot)
        private readonly object _reloadLock = new object();
        private Timer _fullReloadTimer;
        private CancellationTokenSource _fullReloadCts;
        private readonly int _fullReloadDebounceMs = 300;

        #endregion

        #region ====== CTOR ======
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

        #region ====== HELPERS ======

        /// <summary>
        /// Tạo conversationId ổn định cho 2 user (theo thứ tự từ điển).
        /// </summary>
        private string BuildConversationId(string userId1, string userId2)
        {
            int cmp = string.CompareOrdinal(userId1, userId2);
            if (cmp < 0) return userId1 + "_" + userId2;
            return userId2 + "_" + userId1;
        }

        private static int CompareByTime(ChatMessage a, ChatMessage b)
        {
            long ta = (a != null) ? a.Timestamp : 0;
            long tb = (b != null) ? b.Timestamp : 0;

            if (ta < tb) return -1;
            if (ta > tb) return 1;
            return 0;
        }

        private ChatMessage DeserializeMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return JsonConvert.DeserializeObject<ChatMessage>(json);
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, ChatMessage> DeserializeMessageMap(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, ChatMessage>>(json);
            }
            catch
            {
                return null;
            }
        }

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

        #region ====== CONVERSATION (LOAD) ======

        public async Task<List<ChatMessage>> GetConversationAsync(string otherUserId)
        {
            string conversationId = BuildConversationId(_currentUserId, otherUserId);
            return await LoadConversationAsync(conversationId);
        }

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

                    m.MessageId = kv.Key;
                    m.IsMine = string.Equals(m.SenderId, _currentUserId, StringComparison.Ordinal);

                    if (string.IsNullOrWhiteSpace(m.MessageType))
                    {
                        m.MessageType = "text";
                    }

                    list.Add(m);
                }

                list.Sort(CompareByTime);
            }

            return list;
        }

        #endregion

        #region ====== USERS & FRIENDS ======

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

        public async Task<Dictionary<string, User>> GetFriendUsersAsync(string currentLocalId)
        {
            string safeMe = KeySanitizer.SafeKey(currentLocalId);

            FirebaseResponse respFriends = await _firebase.GetAsync("friends/" + safeMe);
            Dictionary<string, bool> friendIds = respFriends.ResultAs<Dictionary<string, bool>>();

            if (friendIds == null || friendIds.Count == 0)
            {
                return new Dictionary<string, User>();
            }

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

        #region ====== REALTIME LISTEN (INCREMENTAL) ======

        /// <summary>
        /// Listen incremental:
        /// - Load initial 1 lần
        /// - Stream "added": append tin mới
        /// - Nếu snapshot/changed/remove => debounce reload full
        /// </summary>
        public async void StartListenConversation(
            string otherUserId,
            Action<List<ChatMessage>> onInitialLoaded,
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            StopListen();

            if (onInitialLoaded == null || onMessageAdded == null)
            {
                return;
            }

            string conversationId = BuildConversationId(_currentUserId, otherUserId);
            _listeningConversationId = conversationId;

            // Load initial 1 lần
            List<ChatMessage> initial;
            try
            {
                initial = await LoadConversationAsync(conversationId);
            }
            catch
            {
                initial = new List<ChatMessage>();
            }

            ResetKnownIds(initial);

            try { onInitialLoaded(initial); } catch { }

            string path = "messages/" + conversationId;

            try
            {
                _stream = await _firebase.OnAsync(
                path,
                added: (s, e, c) => { HandleAddedEvent(conversationId, e, onMessageAdded, onReset); },
                changed: (s, e, c) => { HandleChangedEvent(conversationId, e, onReset); },
                removed: (s, e, c) => { HandleRemovedEvent(conversationId, e, onReset); }
            );

            }
            catch
            {
                // ignore
            }
        }

        private void HandleAddedEvent(string conversationId, ValueAddedEventArgs e, Action<ChatMessage> onMessageAdded,
                                        Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(
                conversationId,
                (e != null) ? e.Path : null,
                (e != null) ? e.Data : null,
                "added",
                onMessageAdded,
                onReset);
        }

        private void HandleChangedEvent(
            string conversationId,
            ValueChangedEventArgs e,
            Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(
                conversationId,
                (e != null) ? e.Path : null,
                (e != null) ? e.Data : null,
                "changed",
                null,
                onReset);
        }

        private void HandleRemovedEvent(
            string conversationId,
            ValueRemovedEventArgs e,
            Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(
                conversationId,
                (e != null) ? e.Path : null,
                 null,
                "removed",
                null,
                onReset);
        }

        private void ProcessStreamEvent(
            string conversationId,
            string path,
            string data,
            string kind, // "added" | "changed" | "removed"
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal))
                    {
                        return;
                    }

                    string p = path ?? string.Empty;
                    string d = data ?? string.Empty;

                    // Snapshot toàn bộ node
                    if (p == "/")
                    {
                        if (string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                        {
                            if (onReset != null)
                            {
                                try { onReset(new List<ChatMessage>()); } catch { }
                            }
                            return;
                        }

                        Dictionary<string, ChatMessage> map = DeserializeMessageMap(d);
                        if (map != null)
                        {
                            List<ChatMessage> list = new List<ChatMessage>();

                            foreach (KeyValuePair<string, ChatMessage> kv in map)
                            {
                                if (kv.Value == null) continue;

                                kv.Value.MessageId = kv.Key;
                                kv.Value.IsMine = string.Equals(kv.Value.SenderId, _currentUserId, StringComparison.Ordinal);

                                if (string.IsNullOrWhiteSpace(kv.Value.MessageType))
                                {
                                    kv.Value.MessageType = "text";
                                }

                                list.Add(kv.Value);
                            }

                            list.Sort(CompareByTime);
                            ResetKnownIds(list);

                            if (onReset != null)
                            {
                                try { onReset(list); } catch { }
                            }
                            return;
                        }

                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    // Path có thể dạng "/-Nabc" hoặc "/-Nabc/field"
                    string trimmed = p.Trim('/');
                    if (string.IsNullOrEmpty(trimmed)) return;

                    // Nếu event đi sâu vào field => reload full (debounce)
                    if (trimmed.IndexOf('/') >= 0)
                    {
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    string msgId = trimmed;

                    // Removed hoặc data null
                    if (string.Equals(kind, "removed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    // changed thường có thể trả scalar/partial => nếu parse fail thì reload
                    ChatMessage m = DeserializeMessage(d);
                    if (m == null)
                    {
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    m.MessageId = msgId;
                    m.IsMine = string.Equals(m.SenderId, _currentUserId, StringComparison.Ordinal);

                    if (string.IsNullOrWhiteSpace(m.MessageType))
                    {
                        m.MessageType = "text";
                    }

                    // Incremental chỉ append cho "added"
                    if (string.Equals(kind, "added", StringComparison.OrdinalIgnoreCase))
                    {
                        bool isNew = MarkKnownIfNew(msgId);
                        if (isNew)
                        {
                            if (onMessageAdded != null)
                            {
                                try { onMessageAdded(m); } catch { }
                            }
                        }
                        return;
                    }

                    // changed => reload full (debounce)
                    DebounceFullReload(conversationId, onReset);
                }
                catch
                {
                    // ignore
                }

                await Task.CompletedTask;
            });
        }

        private void DebounceFullReload(string conversationId, Action<List<ChatMessage>> onReset)
        {
            if (onReset == null) return;

            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal))
            {
                return;
            }

            lock (_reloadLock)
            {
                if (_fullReloadCts != null)
                {
                    try { _fullReloadCts.Cancel(); } catch { }
                    try { _fullReloadCts.Dispose(); } catch { }
                    _fullReloadCts = null;
                }

                _fullReloadCts = new CancellationTokenSource();

                if (_fullReloadTimer != null)
                {
                    try { _fullReloadTimer.Dispose(); } catch { }
                    _fullReloadTimer = null;
                }

                CancellationToken ct = _fullReloadCts.Token;

                _fullReloadTimer = new Timer(
                    async _ =>
                    {
                        try
                        {
                            if (ct.IsCancellationRequested) return;

                            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal))
                            {
                                return;
                            }

                            List<ChatMessage> full = await LoadConversationAsync(conversationId);
                            if (ct.IsCancellationRequested) return;

                            ResetKnownIds(full);

                            try { onReset(full); } catch { }
                        }
                        catch
                        {
                            // ignore
                        }
                    },
                    null,
                    _fullReloadDebounceMs,
                    Timeout.Infinite);
            }
        }

        public void StopListen()
        {
            _listeningConversationId = null;

            lock (_reloadLock)
            {
                if (_fullReloadTimer != null)
                {
                    try { _fullReloadTimer.Dispose(); } catch { }
                    _fullReloadTimer = null;
                }

                if (_fullReloadCts != null)
                {
                    try { _fullReloadCts.Cancel(); } catch { }
                    try { _fullReloadCts.Dispose(); } catch { }
                    _fullReloadCts = null;
                }
            }

            lock (_knownLock)
            {
                _knownMessageIds.Clear();
            }

            if (_stream != null)
            {
                try { _stream.Dispose(); } catch { }
                _stream = null;
            }
        }

        #endregion

        #region ====== SEND MESSAGE ======

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

        #region ====== SEND FILE ======
        /// <summary>
        /// Gửi file/ảnh chung một luồng:
        /// - Nếu là ảnh: không upload Catbox, gửi trực tiếp base64 vào chat.
        /// - Nếu là file thường: upload Catbox.
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
                msg.ImageMimeType = string.IsNullOrWhiteSpace(mime) ? AttachmentClassifier.GetMimeTypeByExtension(filePath) : mime;
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



        public Task SendFileMessageAsync(string toUserId, string filePath)
        {
            // Giữ tương thích chỗ cũ đang gọi SendFileMessageAsync
            return SendAttachmentMessageAsync(toUserId, filePath);
        }


        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            StopListen();
        }

        #endregion

        #region ====== DELETE CONVERSATION ======

        /// <summary>
        /// Xóa toàn bộ lịch sử tin nhắn giữa người dùng hiện tại và đối phương.
        /// </summary>
        public async Task DeleteFullConversationAsync(string otherUserId)
        {
            try
            {
                // 1. Xác định ID cuộc trò chuyện dựa trên 2 ID người dùng
                string conversationId = BuildConversationId(_currentUserId, otherUserId);

                // 2. Đường dẫn node tin nhắn trên Firebase
                string path = "messages/" + conversationId;

                // 3. Thực hiện lệnh DELETE
                FirebaseResponse response = await _firebase.DeleteAsync(path);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    throw new Exception($"Lỗi Firebase: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa lịch sử tin nhắn: " + ex.Message);
            }
        }

        #endregion
    }
}
