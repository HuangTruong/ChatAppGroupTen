using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Attachments;
using ChatApp.Services.FileHost;
using FireSharp;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FirebaseAppConfig = ChatApp.Services.Firebase.FirebaseConfig;

namespace ChatApp.Services.Messages
{
    /// <summary>
    /// MessageService: lớp truy cập Firebase (Get/Push/Delete/Stream) + gửi text/file/image.
    /// Controller sẽ gọi service này để tách bớt phần IO.
    /// </summary>
    public class MessageService : IDisposable
    {
        #region ====== FIELDS ======

        private readonly IFirebaseClient _firebase;

        #endregion

        #region ====== CTOR ======

        public MessageService()
        {
            IFirebaseConfig cfg = new FirebaseConfig();
            cfg.BasePath = FirebaseAppConfig.DatabaseUrl;
            cfg.AuthSecret = string.Empty;

            _firebase = new FirebaseClient(cfg);
        }

        #endregion

        #region ====== BASIC FIREBASE OPS ======

        public Task<FirebaseResponse> GetAsync(string path)
        {
            return _firebase.GetAsync(path);
        }

        public Task<FirebaseResponse> DeleteAsync(string path)
        {
            return _firebase.DeleteAsync(path);
        }

        public Task<PushResponse> PushAsync(string path, object data)
        {
            return _firebase.PushAsync(path, data);
        }

        public Task<EventStreamResponse> ListenAsync(
    string path,
    EventHandler<ValueAddedEventArgs> added,
    EventHandler<ValueChangedEventArgs> changed,
    EventHandler<ValueRemovedEventArgs> removed)
        {
            ValueAddedEventHandler a = null;
            if (added != null)
            {
                a = delegate (object sender, ValueAddedEventArgs e, object context)
                {
                    added(sender, e);
                };
            }

            ValueChangedEventHandler c = null;
            if (changed != null)
            {
                c = delegate (object sender, ValueChangedEventArgs e, object context)
                {
                    changed(sender, e);
                };
            }

            ValueRemovedEventHandler r = null;
            if (removed != null)
            {
                r = delegate (object sender, ValueRemovedEventArgs e, object context)
                {
                    removed(sender, e);
                };
            }

            // gọi positional để khỏi dính named-arg (Changed/changed)
            return _firebase.OnAsync(path, a, c, r);
        }

        #endregion

        #region ====== USERS ======

        public async Task<Dictionary<string, User>> GetAllUsersAsync()
        {
            FirebaseResponse resp = await _firebase.GetAsync("users").ConfigureAwait(false);
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

        public async Task<Dictionary<string, bool>> GetFriendIdsAsync(string safeMe)
        {
            FirebaseResponse resp = await _firebase.GetAsync("friends/" + safeMe).ConfigureAwait(false);
            Dictionary<string, bool> ids = resp.ResultAs<Dictionary<string, bool>>();
            return ids ?? new Dictionary<string, bool>();
        }

        public async Task<User> GetUserAsync(string userId)
        {
            FirebaseResponse resp = await _firebase.GetAsync("users/" + userId).ConfigureAwait(false);
            return resp.ResultAs<User>();
        }

        #endregion

        #region ====== CONVERSATION ======

        public async Task<Dictionary<string, ChatMessage>> GetConversationRawAsync(string conversationId)
        {
            FirebaseResponse resp = await _firebase.GetAsync("messages/" + conversationId).ConfigureAwait(false);
            Dictionary<string, ChatMessage> raw = resp.ResultAs<Dictionary<string, ChatMessage>>();
            return raw ?? new Dictionary<string, ChatMessage>();
        }

        public async Task DeleteConversationAsync(string conversationId)
        {
            await _firebase.DeleteAsync("messages/" + conversationId).ConfigureAwait(false);
        }

        #endregion

        #region ====== SEND ======

        public async Task SendTextAsync(string conversationId, string senderId, string receiverId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            string path = "messages/" + conversationId;

            ChatMessage msg = new ChatMessage();
            msg.SenderId = senderId;
            msg.ReceiverId = receiverId;
            msg.Text = text;
            msg.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            msg.IsMine = true;
            msg.MessageType = "text";

            await _firebase.PushAsync(path, msg).ConfigureAwait(false);
        }

        /// <summary>
        /// Gửi file/ảnh chung một luồng:
        /// - Ảnh: gửi base64 trực tiếp vào chat.
        /// - File thường: upload Catbox (hoặc uploader hiện tại) rồi gửi URL.
        /// </summary>
        public async Task SendAttachmentAsync(string conversationId, string senderId, string receiverId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(receiverId))
            {
                throw new Exception("Chưa chọn người để chat.");
            }

            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                throw new Exception("File không tồn tại.");
            }

            bool isImage = AttachmentClassifier.IsImageFile(filePath, out string mime);

            string path = "messages/" + conversationId;

            ChatMessage msg = new ChatMessage();
            msg.SenderId = senderId;
            msg.ReceiverId = receiverId;
            msg.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            msg.IsMine = true;

            if (isImage)
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

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            // FireSharp client không bắt buộc dispose trong đa số trường hợp.
        }

        #endregion
    }
}
