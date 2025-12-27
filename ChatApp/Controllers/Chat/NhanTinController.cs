using ChatApp.Helpers;
using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Messages;
using FireSharp.EventStreaming;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    /// <summary>
    /// NhanTinController: điều phối logic chat + listen realtime + chống duplicate + debounce reload.
    /// IO Firebase / upload file được đẩy sang MessageService.
    /// </summary>
    public class NhanTinController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly string _currentUserId;
        private readonly string _token;

        private readonly MessageService _messageService;

        private EventStreamResponse _stream;
        private string _listeningConversationId;

        private readonly object _knownLock = new object();
        private readonly HashSet<string> _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

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

            _messageService = new MessageService();
        }

        #endregion

        #region ====== HELPERS ======

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
            return await LoadConversationAsync(conversationId).ConfigureAwait(false);
        }

        private async Task<List<ChatMessage>> LoadConversationAsync(string conversationId)
        {
            Dictionary<string, ChatMessage> raw = await _messageService.GetConversationRawAsync(conversationId).ConfigureAwait(false);

            List<ChatMessage> list = new List<ChatMessage>();

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
            return list;
        }

        #endregion

        #region ====== USERS & FRIENDS ======

        public Task<Dictionary<string, User>> GetAllUsersAsync()
        {
            return _messageService.GetAllUsersAsync();
        }

        public async Task<Dictionary<string, User>> GetFriendUsersAsync(string currentLocalId)
        {
            string safeMe = KeySanitizer.SafeKey(currentLocalId);

            Dictionary<string, bool> friendIds = await _messageService.GetFriendIdsAsync(safeMe).ConfigureAwait(false);
            if (friendIds == null || friendIds.Count == 0)
            {
                return new Dictionary<string, User>();
            }

            if (friendIds.Count > 50)
            {
                Dictionary<string, User> allUsers = await _messageService.GetAllUsersAsync().ConfigureAwait(false);
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
                        User u = await _messageService.GetUserAsync(friendId).ConfigureAwait(false);
                        if (u != null)
                        {
                            u.LocalId = friendId;
                            lock (sync)
                            {
                                result[friendId] = u;
                            }
                        }
                    }
                    catch { }
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

            List<ChatMessage> initial;
            try { initial = await LoadConversationAsync(conversationId).ConfigureAwait(false); }
            catch { initial = new List<ChatMessage>(); }

            ResetKnownIds(initial);
            try { onInitialLoaded(initial); } catch { }

            string path = "messages/" + conversationId;

            try
            {
                _stream = await _messageService.ListenAsync(
                    path,
                    added: (s, e) => { HandleAddedEvent(conversationId, e, onMessageAdded, onReset); },
                    changed: (s, e) => { HandleChangedEvent(conversationId, e, onReset); },
                    removed: (s, e) => { HandleRemovedEvent(conversationId, e, onReset); }
                ).ConfigureAwait(false);
            }
            catch { }
        }

        private void HandleAddedEvent(string conversationId, ValueAddedEventArgs e, Action<ChatMessage> onMessageAdded, Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(conversationId, (e != null) ? e.Path : null, (e != null) ? e.Data : null, "added", onMessageAdded, onReset);
        }

        private void HandleChangedEvent(string conversationId, ValueChangedEventArgs e, Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(conversationId, (e != null) ? e.Path : null, (e != null) ? e.Data : null, "changed", null, onReset);
        }

        private void HandleRemovedEvent(string conversationId, ValueRemovedEventArgs e, Action<List<ChatMessage>> onReset)
        {
            ProcessStreamEvent(conversationId, (e != null) ? e.Path : null, null, "removed", null, onReset);
        }

        private void ProcessStreamEvent(
            string conversationId,
            string path,
            string data,
            string kind,
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;

                    string p = path ?? string.Empty;
                    string d = data ?? string.Empty;

                    // Snapshot toàn bộ node
                    if (p == "/")
                    {
                        if (string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                        {
                            if (onReset != null) { try { onReset(new List<ChatMessage>()); } catch { } }
                            return;
                        }

                        // parse map => nếu fail thì reload full
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    string trimmed = p.Trim('/');
                    if (string.IsNullOrEmpty(trimmed)) return;

                    // Event sâu vào field => reload full
                    if (trimmed.IndexOf('/') >= 0)
                    {
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    string msgId = trimmed;

                    if (string.Equals(kind, "removed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        DebounceFullReload(conversationId, onReset);
                        return;
                    }

                    // Parse message từ JSON
                    ChatMessage m = null;
                    try { m = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMessage>(d); } catch { m = null; }

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

                    if (string.Equals(kind, "added", StringComparison.OrdinalIgnoreCase))
                    {
                        bool isNew = MarkKnownIfNew(msgId);
                        if (isNew && onMessageAdded != null)
                        {
                            try { onMessageAdded(m); } catch { }
                        }
                        return;
                    }

                    DebounceFullReload(conversationId, onReset);
                }
                catch { }

                await Task.CompletedTask;
            });
        }

        private void DebounceFullReload(string conversationId, Action<List<ChatMessage>> onReset)
        {
            if (onReset == null) return;
            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;

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
                            if (!string.Equals(_listeningConversationId, conversationId, StringComparison.Ordinal)) return;

                            List<ChatMessage> full = await LoadConversationAsync(conversationId).ConfigureAwait(false);
                            if (ct.IsCancellationRequested) return;

                            ResetKnownIds(full);
                            try { onReset(full); } catch { }
                        }
                        catch { }
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

        public Task SendMessageAsync(string toUserId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;

            string conversationId = BuildConversationId(_currentUserId, toUserId);
            return _messageService.SendTextAsync(conversationId, _currentUserId, toUserId, text);
        }

        public Task SendAttachmentMessageAsync(string toUserId, string filePath)
        {
            string conversationId = BuildConversationId(_currentUserId, toUserId);
            return _messageService.SendAttachmentAsync(conversationId, _currentUserId, toUserId, filePath);
        }

        public Task SendFileMessageAsync(string toUserId, string filePath)
        {
            return SendAttachmentMessageAsync(toUserId, filePath);
        }

        #endregion

        #region ====== DELETE CONVERSATION ======

        public async Task DeleteFullConversationAsync(string otherUserId)
        {
            try
            {
                string conversationId = BuildConversationId(_currentUserId, otherUserId);
                await _messageService.DeleteConversationAsync(conversationId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa lịch sử tin nhắn: " + ex.Message);
            }
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            StopListen();
            try { _messageService.Dispose(); } catch { }
        }

        #endregion
    }
}
