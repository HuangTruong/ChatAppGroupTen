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
    /// Controller nhắn tin nhóm:
    /// - Quản lý nhóm (list/create)
    /// - Load lịch sử nhóm
    /// - Listen realtime (incremental + debounce reload full)
    /// - Gửi text / ảnh / file trong nhóm
    /// </summary>
    public class NhanTinNhomController : IDisposable
    {
        #region ====== BIẾN THÀNH VIÊN (FIELDS) ======

        private readonly string _currentUserId;
        private readonly string _token;

        private readonly GroupService _groupService;
        private readonly GroupMessageService _messageService;

        private readonly IFirebaseClient _firebase;

        private EventStreamResponse _stream;
        private string _listeningGroupId;

        // Chống duplicate khi stream "added"
        private readonly object _knownLock = new object();
        private HashSet<string> _knownMessageIds = new HashSet<string>(StringComparer.Ordinal);

        // Debounce reload full khi changed/removed/snapshot/field-change
        private readonly object _reloadLock = new object();
        private Timer _reloadDebounceTimer;

        private bool _ignoreFirstSnapshot;
        private Action<List<ChatMessage>> _onReset;

        #endregion

        #region ====== KHỞI TẠO (CTOR) ======

        /// <summary>
        /// Khởi tạo controller nhắn tin nhóm.
        /// </summary>
        /// <param name="currentUserId">localId của user hiện tại.</param>
        /// <param name="token">token đăng nhập (để dành nếu cần).</param>
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

        #region ====== NHÓM (DANH SÁCH / TẠO MỚI) ======

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
        public Task<string> CreateGroupAsync(string _currentUserId, string groupName, List<string> memberIds)
        {
            return _groupService.CreateGroupAsync(_currentUserId, groupName, memberIds, _token);
        }

        #endregion

        #region ====== REALTIME (LẮNG NGHE NHÓM) ======

        /// <summary>
        /// Listen realtime:
        /// - Load initial 1 lần
        /// - "added": append tin mới (chống duplicate)
        /// - "changed/removed/snapshot/field-change": debounce reload full
        /// </summary>

        /// <summary>
        /// Listen realtime đơn giản cho nhóm:
        /// - Load initial 1 lần
        /// - Chỉ xử lý event "added" (tin mới) => UI append mượt
        /// Lưu ý: edit/delete message sẽ không tự update (đổi lại code đơn giản + mượt).
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



            _onReset = onReset;
            _ignoreFirstSnapshot = true;
            // 1) Load initial 1 lần
            List<ChatMessage> initial = await SafeLoadHistoryAsync(gid).ConfigureAwait(false);
            ResetKnownIds(initial);

            try { onInitialLoaded(initial); } catch { }

            // 2) Stream: chỉ quan tâm "added"
            string path = "groupMessages/" + gid;

            try
            {
                _stream = await _firebase.OnAsync(
                    path,
                    added: (s, e, c) => { OnAddedEvent(gid, e, onMessageAdded); },
                    changed: (s, e, c) => { OnChangedEvent(gid, e); },
                    removed: (s, e, c) => { OnRemovedEvent(gid, e); }
                ).ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Dừng lắng nghe nhóm hiện tại, hủy timer/cts và dispose stream.
        /// </summary>

        /// <summary>
        /// Chỉ nhận tin mới (added) và chống duplicate bằng knownIds.
        /// </summary>
        /// <summary>
        /// Nhận tin mới (added) và chống duplicate bằng knownIds.
        /// Đồng thời xử lý snapshot "/" và event sâu hơn để đảm bảo không miss patch.
        /// </summary>
        /// <summary>
        /// Nhận event "added" từ stream.
        /// - "/" (snapshot)   : ignore lần đầu (vì đã load initial), lần sau thì reset.
        /// - "/-N..." (message): deserialize và append nếu chưa seen.
        /// - "/-N.../field"  : schedule reload full (debounce) để không miss patch.
        /// </summary>
        private void OnAddedEvent(string groupId, ValueAddedEventArgs e, Action<ChatMessage> onMessageAdded)
        {
            try
            {
                if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;
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

                    HandleSnapshotReset(groupId, d);
                    return;
                }

                string trimmed = p.Trim('/');
                if (string.IsNullOrEmpty(trimmed)) return;

                // Patch sâu: /messageId/field
                if (trimmed.IndexOf('/') >= 0)
                {
                    ScheduleReload(groupId);
                    return;
                }

                if (string.Equals(d, "null", StringComparison.OrdinalIgnoreCase))
                {
                    ScheduleReload(groupId);
                    return;
                }

                string msgId = trimmed;

                GroupMessageService.GroupMessageData raw = TryDeserializeOne(d);
                if (raw == null)
                {
                    ScheduleReload(groupId);
                    return;
                }

                if (MarkKnownIfNew(msgId))
                {
                    ChatMessage msg = ConvertOne(groupId, msgId, raw);
                    if (onMessageAdded != null)
                    {
                        try { onMessageAdded(msg); } catch { }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void OnChangedEvent(string groupId, ValueChangedEventArgs e)
        {
            try
            {
                if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;
                if (e == null) return;

                string p = e.Path ?? string.Empty;
                string d = e.Data ?? string.Empty;

                // Có thể bắn snapshot hoặc patch
                if (p == "/")
                {
                    if (_ignoreFirstSnapshot)
                    {
                        _ignoreFirstSnapshot = false;
                        return;
                    }

                    HandleSnapshotReset(groupId, d);
                    return;
                }

                ScheduleReload(groupId);
            }
            catch
            {
                // ignore
            }
        }

        private void OnRemovedEvent(string groupId, ValueRemovedEventArgs e)
        {
            if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;
            ScheduleReload(groupId);
        }

        private void HandleSnapshotReset(string groupId, string json)
        {
            if (_onReset == null) return;
            if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;

            if (string.IsNullOrWhiteSpace(json) || string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
            {
                List<ChatMessage> empty = new List<ChatMessage>();
                ResetKnownIds(empty);
                try { _onReset(empty); } catch { }
                return;
            }

            Dictionary<string, GroupMessageService.GroupMessageData> map = TryDeserializeMap(json);
            if (map == null)
            {
                ScheduleReload(groupId);
                return;
            }

            List<ChatMessage> list = ConvertToList(groupId, map);
            ResetKnownIds(list);
            try { _onReset(list); } catch { }
        }

        private void ScheduleReload(string groupId)
        {
            if (_onReset == null) return;
            if (string.IsNullOrEmpty(groupId)) return;
            if (!string.Equals(_listeningGroupId, groupId, StringComparison.Ordinal)) return;

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
            string gid = _listeningGroupId;
            Action<List<ChatMessage>> cb = _onReset;

            if (string.IsNullOrEmpty(gid) || cb == null) return;

            List<ChatMessage> full = await SafeLoadHistoryAsync(gid).ConfigureAwait(false);
            ResetKnownIds(full);
            try { cb(full); } catch { }
        }

        public void StopListen()
        {
            _listeningGroupId = null;
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

        /// <summary>
        /// Xử lý event stream theo 3 loại:
        /// - Snapshot "/" (có thể map đầy đủ hoặc null)
        /// - Event theo messageId ("/-Nxxx")
        /// - Event sâu hơn ("/-Nxxx/field") => debounce reload full
        /// </summary>

        /// <summary>
        /// Debounce reload full lịch sử nhóm:
        /// gom nhiều changed/removed liên tiếp thành 1 lần tải lại.
        /// </summary>

        #endregion

        #region ====== TẢI LỊCH SỬ NHÓM (LOAD) ======

        /// <summary>
        /// Load lịch sử nhóm (an toàn, fail => list rỗng).
        /// </summary>
        private async Task<List<ChatMessage>> SafeLoadHistoryAsync(string groupId)
        {
            try { return await LoadGroupHistoryAsync(groupId).ConfigureAwait(false); }
            catch { return new List<ChatMessage>(); }
        }

        /// <summary>
        /// Load full lịch sử nhóm từ node groupMessages/{groupId}.
        /// </summary>
        private async Task<List<ChatMessage>> LoadGroupHistoryAsync(string groupId)
        {
            string gid = KeySanitizer.SafeKey(groupId);

            FirebaseResponse resp = await _firebase.GetAsync("groupMessages/" + gid).ConfigureAwait(false);
            Dictionary<string, GroupMessageService.GroupMessageData> raw =
                resp.ResultAs<Dictionary<string, GroupMessageService.GroupMessageData>>();

            return ConvertToList(gid, raw);
        }

        #endregion

        #region ====== GỬI TIN NHẮN NHÓM (TEXT / ẢNH / FILE) ======

        /// <summary>
        /// Gửi tin nhắn text trong nhóm và trả về ChatMessage để UI append ngay.
        /// </summary>
        public async Task<ChatMessage> SendGroupMessageAsync(string groupId, string text)
        {
            string gid = KeySanitizer.SafeKey(groupId);
            if (string.IsNullOrWhiteSpace(gid)) throw new Exception("Chưa chọn nhóm để chat.");
            if (string.IsNullOrWhiteSpace(text)) return null;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Đảm bảo mapping groupsByUser tồn tại (phòng trường hợp node bị mất)
            try { await _groupService.EnsureMembershipLinkAsync(gid, _currentUserId, _token).ConfigureAwait(false); } catch { }


            string mid = await _messageService.SendTextAsync(
                gid,
                _currentUserId,
                text.Trim(),
                now,
                _token).ConfigureAwait(false);

            // best-effort update preview
            try { await _groupService.UpdateLastMessageAsync(gid, text.Trim(), now, _token).ConfigureAwait(false); } catch { }
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
        /// - Ảnh: gửi base64 trực tiếp.
        /// - File: upload (Catbox) và gửi URL.
        /// </summary>
        public async Task<ChatMessage> SendGroupAttachmentMessageAsync(string groupId, string filePath)
        {
            string gid = KeySanitizer.SafeKey(groupId);

            try { await _groupService.EnsureMembershipLinkAsync(gid, _currentUserId, _token).ConfigureAwait(false); } catch { }

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
        /// Giữ tương thích chỗ cũ đang gọi SendGroupFileMessageAsync.
        /// </summary>
        public Task<ChatMessage> SendGroupFileMessageAsync(string groupId, string filePath)
        {
            return SendGroupAttachmentMessageAsync(groupId, filePath);
        }

        #endregion

        #region ====== TIỆN ÍCH NỘI BỘ (HELPERS) ======

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