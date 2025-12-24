using ChatApp.Controls;
using ChatApp.Models.Groups;
using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller quản lý session chat hiện tại:
    /// - Đang chat 1-1 hay nhóm?
    /// - Start/Stop listen đúng controller
    /// - RenderInitial / Append qua ChatViewController
    /// - Update title/status/avatar
    /// - Send text/file theo đúng mode
    /// </summary>
    public class ChatSessionController : IDisposable
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly string _currentUserId;
        private readonly string _token;
        private readonly Control _uiOwner;

        private readonly Label _lblTitle;
        private readonly Label _lblStatus;
        private readonly PictureBox _pbAvatar;

        private readonly Func<System.Drawing.Image> _getAvatarPlaceholder;

        private readonly AvatarController _avatarController;
        private readonly AuthService _authService;

        private readonly NhanTinController _dmController;
        private readonly NhanTinNhomController _groupController;

        private readonly ChatViewController _chatView;
        private readonly ChatBubbleController _bubbleController;

        private string _currentChatId;
        private bool _isGroupChat;

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public ChatSessionController(
            string currentUserId,
            string token,
            Control uiOwner,
            Label lblTitle,
            Label lblStatus,
            PictureBox pbAvatar,
            Func<System.Drawing.Image> getAvatarPlaceholder,
            AvatarController avatarController,
            AuthService authService,
            NhanTinController directMessageController,
            NhanTinNhomController groupMessageController,
            ChatViewController chatViewController,
            ChatBubbleController chatBubbleController)
        {
            _currentUserId = currentUserId;
            _token = token;
            _uiOwner = uiOwner;

            _lblTitle = lblTitle;
            _lblStatus = lblStatus;
            _pbAvatar = pbAvatar;

            _getAvatarPlaceholder = getAvatarPlaceholder;

            _avatarController = avatarController;
            _authService = authService;

            _dmController = directMessageController;
            _groupController = groupMessageController;

            _chatView = chatViewController;
            _bubbleController = chatBubbleController;

            _currentChatId = null;
            _isGroupChat = false;
        }

        #endregion

        #region ====== SỰ KIỆN CLICK ITEM HỘI THOẠI ======

        public void OnConversationItemClicked(object sender, EventArgs e, string groupTagPrefix, ConversationListController listController)
        {
            Conversations item = sender as Conversations;
            if (item == null) return;

            string tag = item.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            if (!string.IsNullOrEmpty(groupTagPrefix) && tag.StartsWith(groupTagPrefix, StringComparison.Ordinal))
            {
                string gid = tag.Substring(groupTagPrefix.Length);
                OpenGroupConversation(gid, listController);
                return;
            }

            OpenDirectConversation(tag, listController);
        }

        #endregion

        #region ====== MỞ CHAT 1-1 ======

        public void OpenDirectConversation(string otherUserId, ConversationListController listController)
        {
            if (string.IsNullOrWhiteSpace(otherUserId)) return;

            _currentChatId = otherUserId;
            _isGroupChat = false;

            // stop group listen
            try { _groupController.StopListen(); } catch { }

            // set title from list cache
            string peerName = string.Empty;
            User u;
            if (listController != null && listController.Friends != null && listController.Friends.TryGetValue(otherUserId, out u))
            {
                peerName = ChatTextFormatter.FormatUserFullName(u);
            }

            SetTitleSafe(peerName);
            SetStatusSafe(string.Empty);

            // load avatar center
            try { _ = _avatarController.LoadAvatarToPictureBoxAsync(otherUserId, _pbAvatar, _getAvatarPlaceholder()); } catch { }

            _chatView.Clear();
            _bubbleController.SetContext(false, peerName);

            _ = UpdateStatusAsync(otherUserId);

            _dmController.StartListenConversation(
                otherUserId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    UiInvoke(delegate { RenderInitialIfMatch(initial, otherUserId); });
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    UiInvoke(delegate { AppendIfMatch(msg, otherUserId); });
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    UiInvoke(delegate { RenderInitialIfMatch(full, otherUserId); });
                });
        }

        #endregion

        #region ====== MỞ CHAT NHÓM ======

        public void OpenGroupConversation(string groupId, ConversationListController listController)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            _currentChatId = groupId;
            _isGroupChat = true;

            // stop dm listen
            try { _dmController.StopListen(); } catch { }

            // title from list cache
            string title = "Nhóm chat";
            GroupInfo g;
            if (listController != null && listController.Groups != null && listController.Groups.TryGetValue(groupId, out g))
            {
                if (g != null && !string.IsNullOrWhiteSpace(g.Name))
                    title = g.Name;
            }

            SetTitleSafe(title);
            SetStatusSafe(string.Empty);

            // Avatar nhóm: để placeholder (tuỳ bạn muốn custom sau)
            try { if (_pbAvatar != null) _pbAvatar.Image = _getAvatarPlaceholder(); } catch { }

            _chatView.Clear();
            _bubbleController.SetContext(true, string.Empty);

            _groupController.StartListenGroup(
                groupId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    UiInvoke(delegate { RenderInitialIfMatch(initial, groupId); });
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    UiInvoke(delegate { AppendIfMatch(msg, groupId); });
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    UiInvoke(delegate { RenderInitialIfMatch(full, groupId); });
                });
        }

        public void OpenGroupConversationById(string groupId, ConversationListController listController)
        {
            OpenGroupConversation(groupId, listController);
        }

        #endregion

        #region ====== RENDER AN TOÀN THEO SESSION ======

        private void RenderInitialIfMatch(IList<ChatMessage> messages, string ownerKey)
        {
            if (!string.Equals(_currentChatId, ownerKey, StringComparison.Ordinal)) return;
            _chatView.RenderInitial(messages, ownerKey);
        }

        private void AppendIfMatch(ChatMessage msg, string ownerKey)
        {
            if (msg == null) return;
            if (!string.Equals(_currentChatId, ownerKey, StringComparison.Ordinal)) return;

            // Realtime chuẩn: CHỈ append khi listener bắn event
            _chatView.QueueAppend(msg, ownerKey);
        }

        #endregion

        #region ====== CẬP NHẬT TRẠNG THÁI ======

        private async Task UpdateStatusAsync(string otherUserId)
        {
            try
            {
                string st = await _authService.GetStatusAsync(otherUserId).ConfigureAwait(false);
                bool online = string.Equals(st, "online", StringComparison.OrdinalIgnoreCase);

                UiInvoke(delegate
                {
                    SetStatusSafe(online ? "Online" : "Offline");
                });
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ====== GỬI TIN NHẮN / FILE ======

        /// <summary>
        /// Realtime chuẩn:
        /// - KHÔNG append local trước khi gửi.
        /// - UI sẽ được cập nhật bởi listener (onMessageAdded) khi Firebase ghi xong.
        /// </summary>
        public async Task SendTextAsync(string text)
        {
            string noiDung = (text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(noiDung)) return;

            if (string.IsNullOrEmpty(_currentChatId))
            {
                MessageBox.Show(_uiOwner, "Vui lòng chọn người hoặc nhóm cần nhắn tin ở danh sách bên trái.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_isGroupChat)
                {
                    // Nhóm: gửi lên Firebase, listener sẽ bắn event để append
                    await _groupController.SendGroupMessageAsync(_currentChatId, noiDung).ConfigureAwait(true);
                }
                else
                {
                    // 1-1: gửi lên Firebase, listener sẽ bắn event để append
                    await _dmController.SendMessageAsync(_currentChatId, noiDung).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_uiOwner, "Lỗi gửi tin nhắn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task PickAndSendAttachmentAsync(IWin32Window owner)
        {
            if (string.IsNullOrEmpty(_currentChatId))
            {
                MessageBox.Show(owner, "Chọn người/nhóm để chat trước đã.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file để gửi";
                ofd.Filter = "All files (*.*)|*.*";

                if (ofd.ShowDialog(owner) != DialogResult.OK) return;

                try
                {
                    if (_isGroupChat)
                    {
                        // Realtime "chuẩn": chỉ gửi lên Firebase, KHÔNG append local.
                        // Mọi client (kể cả người gửi) sẽ nhận lại qua listener -> UI append 1 lần, không trùng.
                        await _groupController.SendGroupAttachmentMessageAsync(_currentChatId, ofd.FileName).ConfigureAwait(true);
                    }
                    else
                    {
                        await _dmController.SendAttachmentMessageAsync(_currentChatId, ofd.FileName).ConfigureAwait(true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, "Lỗi gửi file: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region ====== HỖ TRỢ CẬP NHẬT UI ======

        private void SetTitleSafe(string title)
        {
            if (_lblTitle == null) return;
            _lblTitle.Text = title ?? string.Empty;
        }

        private void SetStatusSafe(string status)
        {
            if (_lblStatus == null) return;
            _lblStatus.Text = status ?? string.Empty;
        }

        private void UiInvoke(Action a)
        {
            if (a == null) return;

            try
            {
                if (_uiOwner == null || _uiOwner.IsDisposed) return;

                if (_uiOwner.InvokeRequired)
                {
                    _uiOwner.BeginInvoke(a);
                }
                else
                {
                    a();
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        public void Dispose()
        {
            try { _dmController.StopListen(); } catch { }
            try { _groupController.StopListen(); } catch { }
        }

        #endregion
    }
}
