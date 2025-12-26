using ChatApp.Helpers;
using ChatApp.Models.Groups;
using ChatApp.Models.Messages;
using ChatApp.Services.FileHost;
using ChatApp.Services.Attachments;
using ChatApp.Services.Firebase;
using FireSharp;
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
using FireSharpFirebaseConfig = FireSharp.Config.FirebaseConfig;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller nhắn tin nhóm (realtime giống 1-1):
    /// - Load lịch sử 1 lần
    /// - Listen realtime (added/changed/removed) trên node groupMessages/{groupId}
    /// - Send text / file
    /// </summary>
    public class NhanTinNhomController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly string _currentUserId;
        private readonly string _token;

        private readonly GroupService _groupService;
        private readonly GroupMessageService _messageService;

        private readonly IFirebaseClient _firebase;

        private EventStreamResponse _stream;
        private string _listeningGroupId;

        private readonly object _knownLock = new object();
        private HashSet<string> _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

        private readonly object _reloadLock = new object();
        private Timer _reloadTimer;
        private CancellationTokenSource _reloadCts;
        private const int FULL_RELOAD_DEBOUNCE_MS = 350;

        #endregion

        #region ====== CTOR ======

        public NhanTinNhomController(string currentUserId, string token)
        {
            _currentUserId = KeySanitizer.SafeKey(currentUserId);
            _token = token;

            _groupService = new GroupService();
            _messageService = new GroupMessageService();

            IFirebaseConfig cfg = new FireSharpFirebaseConfig
            {
                AuthSecret = string.Empty,
                BasePath = FirebaseAppConfig.DatabaseUrl
            };

            _firebase = new FirebaseClient(cfg);
        }

        #endregion

        #region ====== GROUP LIST / CREATE ======

        /// <summary>
        /// Lấy danh sách nhóm của user hiện tại.
        /// </summary>
        public Task<Dictionary<string, GroupInfo>> GetMyGroupsAsync()
        {
            return _groupService.GetMyGroupsAsync(_currentUserId, _token);
        }

        /// <summary>
        /// Tạo nhóm mới (bao gồm creator).
        /// </summary>
        //public Task<string> CreateGroupAsync(string groupName, List<string> memberIds)
        //{
        //    return _groupService.CreateGroupAsync(_currentUserId, groupName, memberIds, _token);
        //}
        public Task<string> CreateGroupAsync(string groupName, List<string> memberIds, string avatarBase64)
        {
            return _groupService.CreateGroupAsync(_currentUserId, groupName, memberIds, avatarBase64, _token);
        }

        #endregion

        #region ====== REALTIME LISTEN GROUP ======

        /// <summary>
        /// Listen realtime giống 1-1:
        /// - Load initial 1 lần
        /// - Stream "added": append tin mới
        /// - changed/removed/snapshot => debounce reload full
        /// </summary>
        public async void StartListenGroup(
            string groupId,
            Action<List<ChatMessage>> onInitialLoaded,
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            StopListen();

            if (onInitialLoaded == null || onMessageAdded == null) return;

            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid)) return;

            _listeningGroupId = gid;

            // 1) Load initial
            List<ChatMessage> initial = await SafeLoadHistoryAsync(gid).ConfigureAwait(false);
            ResetKnownIds(initial);

            try { onInitialLoaded(initial); } catch { }

            // 2) Realtime stream
            string path = "groupMessages/" + gid;

            try
            {
                _stream = await _firebase.OnAsync(
                    path,
                    added: (s, e, c) => OnStreamEvent(gid, e != null ? e.Path : null, e != null ? e.Data : null, "added", onMessageAdded, onReset),
                    changed: (s, e, c) => OnStreamEvent(gid, e != null ? e.Path : null, e != null ? e.Data : null, "changed", null, onReset),
                    removed: (s, e, c) => OnStreamEvent(gid, e != null ? e.Path : null, null, "removed", null, onReset)
                ).ConfigureAwait(false);
            }
            catch
            {
                // stream fail: không crash UI
            }
        }

        private void OnStreamEvent(
            string groupId,
            string path,
            string data,
            string kind, // "added" | "changed" | "removed"
            Action<ChatMessage> onMessageAdded,
            Action<List<ChatMessage>> onReset)
        {
            Task.Run(async delegate
            {
                try
                {
                    if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;

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

                        Dictionary<string, GroupMessageService.GroupMessageData> map = TryDeserializeMap(d);
                        if (map != null)
                        {
                            List<ChatMessage> list = ConvertToList(groupId, map);
                            ResetKnownIds(list);
                            if (onReset != null) { try { onReset(list); } catch { } }
                            return;
                        }

                        DebounceReload(groupId, onReset);
                        return;
                    }

                    string trimmed = p.Trim('/');
                    if (string.IsNullOrEmpty(trimmed)) return;

                    // event đi sâu vào field: "/-Nxxx/field" => reload full
                    if (trimmed.IndexOf('/') >= 0)
                    {
                        DebounceReload(groupId, onReset);
                        return;
                    }

                    string msgId = trimmed;

                    if (string.Equals(kind, "removed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        DebounceReload(groupId, onReset);
                        return;
                    }

                    GroupMessageService.GroupMessageData raw = TryDeserializeOne(d);
                    if (raw == null)
                    {
                        DebounceReload(groupId, onReset);
                        return;
                    }

                    // chỉ append incremental cho "added"
                    if (string.Equals(kind, "added", StringComparison.OrdinalIgnoreCase))
                    {
                        if (MarkKnownIfNew(msgId))
                        {
                            ChatMessage msg = ConvertOne(groupId, msgId, raw);
                            if (onMessageAdded != null) { try { onMessageAdded(msg); } catch { } }
                        }
                        return;
                    }

                    // changed => reload
                    DebounceReload(groupId, onReset);
                }
                catch
                {
                    // ignore
                }

                await Task.CompletedTask;
            });
        }

        private void DebounceReload(string groupId, Action<List<ChatMessage>> onReset)
        {
            if (onReset == null) return;
            if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;

            lock (_reloadLock)
            {
                if (_reloadCts != null)
                {
                    try { _reloadCts.Cancel(); } catch { }
                    try { _reloadCts.Dispose(); } catch { }
                    _reloadCts = null;
                }

                _reloadCts = new CancellationTokenSource();
                CancellationToken ct = _reloadCts.Token;

                if (_reloadTimer != null)
                {
                    try { _reloadTimer.Dispose(); } catch { }
                    _reloadTimer = null;
                }

                _reloadTimer = new Timer(
                    async _ =>
                    {
                        try
                        {
                            if (ct.IsCancellationRequested) return;
                            if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;

                            List<ChatMessage> full = await SafeLoadHistoryAsync(groupId).ConfigureAwait(false);
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
                    FULL_RELOAD_DEBOUNCE_MS,
                    Timeout.Infinite);
            }
        }

        public void StopListen()
        {
            _listeningGroupId = null;

            lock (_reloadLock)
            {
                if (_reloadTimer != null)
                {
                    try { _reloadTimer.Dispose(); } catch { }
                    _reloadTimer = null;
                }

                if (_reloadCts != null)
                {
                    try { _reloadCts.Cancel(); } catch { }
                    try { _reloadCts.Dispose(); } catch { }
                    _reloadCts = null;
                }
            }

            if (_stream != null)
            {
                try { _stream.Dispose(); } catch { }
                _stream = null;
            }
        }

        private async Task<List<ChatMessage>> SafeLoadHistoryAsync(string groupId)
        {
            try
            {
                return await LoadGroupHistoryAsync(groupId).ConfigureAwait(false);
            }
            catch
            {
                return new List<ChatMessage>();
            }
        }

        private async Task<List<ChatMessage>> LoadGroupHistoryAsync(string groupId)
        {
            string gid = KeySanitizer.SafeKey(groupId);

            FirebaseResponse resp = await _firebase.GetAsync("groupMessages/" + gid).ConfigureAwait(false);
            Dictionary<string, GroupMessageService.GroupMessageData> raw =
                resp.ResultAs<Dictionary<string, GroupMessageService.GroupMessageData>>();

            return ConvertToList(gid, raw);
        }

        #endregion

        #region ====== SEND TEXT / FILE ======

        /// <summary>
        /// Gửi tin nhắn text trong nhóm và trả về ChatMessage để UI append ngay.
        /// </summary>
        public async Task<ChatMessage> SendGroupMessageAsync(string groupId, string text)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid)) throw new Exception("Chưa chọn nhóm để chat.");
            if (string.IsNullOrWhiteSpace(text)) return null;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            string mid = await _messageService.SendTextAsync(
                gid,
                _currentUserId,
                text.Trim(),
                now,
                _token).ConfigureAwait(false);

            // best-effort update preview
            try { await _groupService.UpdateLastMessageAsync(gid, text.Trim(), now, _token).ConfigureAwait(false); } catch { }

            if (!string.IsNullOrEmpty(mid)) MarkKnownIfNew(mid);

            ChatMessage msg = new ChatMessage();
            msg.MessageId = mid;
            msg.SenderId = _currentUserId;
            msg.ReceiverId = gid;
            msg.Text = text.Trim();
            msg.Timestamp = now;
            msg.IsMine = true;
            msg.MessageType = "text";
            return msg;
        }

        /// <summary>
        /// Gửi file/ảnh trong nhóm:
        /// - Ảnh: không upload Catbox, gửi trực tiếp base64.
        /// - File thường: upload Catbox.
        /// </summary>
        public async Task<ChatMessage> SendGroupAttachmentMessageAsync(string groupId, string filePath)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid)) throw new Exception("Chưa chọn nhóm để chat.");

            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists) throw new Exception("File không tồn tại.");

            bool laAnh = AttachmentClassifier.IsImageFile(filePath, out string mime);

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (laAnh)
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(bytes);

                string mid = await _messageService.SendImageAsync(
                    gid,
                    _currentUserId,
                    fi.Name,
                    fi.Length,
                    string.IsNullOrWhiteSpace(mime) ? AttachmentClassifier.GetMimeTypeByExtension(filePath) : mime,
                    base64,
                    now,
                    _token).ConfigureAwait(false);

                try { await _groupService.UpdateLastMessageAsync(gid, "[Ảnh] " + fi.Name, now, _token).ConfigureAwait(false); } catch { }

                if (!string.IsNullOrEmpty(mid)) MarkKnownIfNew(mid);

                ChatMessage msg = new ChatMessage();
                msg.MessageId = mid;
                msg.SenderId = _currentUserId;
                msg.ReceiverId = gid;
                msg.Text = string.Empty;
                msg.Timestamp = now;
                msg.IsMine = true;

                msg.MessageType = "image";
                msg.FileName = fi.Name;
                msg.FileSize = fi.Length;
                msg.ImageMimeType = string.IsNullOrWhiteSpace(mime) ? AttachmentClassifier.GetMimeTypeByExtension(filePath) : mime;
                msg.ImageBase64 = base64;

                return msg;
            }
            else
            {
                FileAttachmentUploader uploader = new FileAttachmentUploader();
                string url = await uploader.UploadAsync(filePath).ConfigureAwait(false);

                string mid = await _messageService.SendFileAsync(
                    gid,
                    _currentUserId,
                    fi.Name,
                    fi.Length,
                    url,
                    now,
                    _token).ConfigureAwait(false);

                try { await _groupService.UpdateLastMessageAsync(gid, "[File] " + fi.Name, now, _token).ConfigureAwait(false); } catch { }

                if (!string.IsNullOrEmpty(mid)) MarkKnownIfNew(mid);

                ChatMessage msg = new ChatMessage();
                msg.MessageId = mid;
                msg.SenderId = _currentUserId;
                msg.ReceiverId = gid;
                msg.Text = string.Empty;
                msg.Timestamp = now;
                msg.IsMine = true;
                msg.MessageType = "file";
                msg.FileName = fi.Name;
                msg.FileSize = fi.Length;
                msg.FileUrl = url;

                return msg;
            }
        }

        /// <summary>
        /// Gửi file trong nhóm.
        /// </summary>
        public Task<ChatMessage> SendGroupFileMessageAsync(string groupId, string filePath)
        {
            // Giữ tương thích chỗ cũ đang gọi SendGroupFileMessageAsync
            return SendGroupAttachmentMessageAsync(groupId, filePath);
        }


        #endregion

        #region ====== INTERNAL HELPERS ======

        private List<ChatMessage> ConvertToList(string groupId, Dictionary<string, GroupMessageService.GroupMessageData> dict)
        {
            List<ChatMessage> list = new List<ChatMessage>();
            if (dict == null || dict.Count == 0) return list;

            foreach (KeyValuePair<string, GroupMessageService.GroupMessageData> kv in dict)
            {
                if (kv.Value == null) continue;
                list.Add(ConvertOne(groupId, kv.Key, kv.Value));
            }

            list.Sort(CompareByTime);
            return list;
        }

        private ChatMessage ConvertOne(string groupId, string messageId, GroupMessageService.GroupMessageData d)
        {
            ChatMessage msg = new ChatMessage();
            msg.MessageId = messageId;
            msg.SenderId = d.SenderId;
            msg.ReceiverId = groupId;
            msg.Text = d.Content;
            msg.Timestamp = d.Timestamp;

            msg.IsMine = string.Equals(
                KeySanitizer.SafeKey(d.SenderId),
                KeySanitizer.SafeKey(_currentUserId),
                StringComparison.Ordinal);

            msg.MessageType = string.IsNullOrWhiteSpace(d.Type) ? "text" : d.Type;
            msg.FileName = d.FileName;
            msg.FileSize = d.FileSize;
            msg.FileUrl = d.FileUrl;
            return msg;
        }

        private static int CompareByTime(ChatMessage a, ChatMessage b)
        {
            long ta = a != null ? a.Timestamp : 0;
            long tb = b != null ? b.Timestamp : 0;
            return ta.CompareTo(tb);
        }

        private void ResetKnownIds(List<ChatMessage> list)
        {
            lock (_knownLock)
            {
                _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

                if (list == null) return;
                foreach (ChatMessage m in list)
                {
                    if (m == null) continue;
                    if (!string.IsNullOrEmpty(m.MessageId)) _knownMessageIds.Add(m.MessageId);
                }
            }
        }

        private bool MarkKnownIfNew(string messageId)
        {
            if (string.IsNullOrWhiteSpace(messageId)) return false;

            lock (_knownLock)
            {
                if (_knownMessageIds.Contains(messageId)) return false;
                _knownMessageIds.Add(messageId);
                return true;
            }
        }

        private static Dictionary<string, GroupMessageService.GroupMessageData> TryDeserializeMap(string json)
        {
            try { return JsonConvert.DeserializeObject<Dictionary<string, GroupMessageService.GroupMessageData>>(json); }
            catch { return null; }
        }

        private static GroupMessageService.GroupMessageData TryDeserializeOne(string json)
        {
            try { return JsonConvert.DeserializeObject<GroupMessageService.GroupMessageData>(json); }
            catch { return null; }
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            StopListen();
        }

        #endregion
    }
}
