using ChatApp.Controls;
using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller tạo MessageBubbles và gắn click:
    /// - File: click -> FileDownloadController
    /// - Image: click -> ImageViewerController + load thumbnail
    /// - Group sender name: lấy từ GroupSenderNameController và update lại bubble khi có tên
    /// </summary>
    public class ChatBubbleController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly string _currentUserId;
        private readonly Control _uiOwner;

        private readonly AuthService _authService;

        private readonly GroupSenderNameController _groupSenderNameController;
        private readonly ImageThumbController _imageThumbController;
        private readonly FileDownloadController _fileDownloadController;
        private readonly ImageViewerController _imageViewerController;

        // (Legacy) Container dạng FlowLayoutPanel (nếu bạn vẫn dùng cách cũ)
        private FlowLayoutPanel _container;

        // Root để duyệt và update bubble đã render (hỗ trợ cả Panel/row panel)
        private Control _renderRoot;

        private bool _isGroupChat;
        private string _peerDisplayName;

        private Image _avatarPlaceholder;

        #endregion

        #region ====== CTOR ======

        public ChatBubbleController(
            string currentUserId,
            Control uiOwner,
            AuthService authService,
            GroupSenderNameController groupSenderNameController,
            ImageThumbController imageThumbController,
            FileDownloadController fileDownloadController,
            ImageViewerController imageViewerController)
        {
            _currentUserId = currentUserId;
            _uiOwner = uiOwner;

            _authService = authService;

            _groupSenderNameController = groupSenderNameController;
            _imageThumbController = imageThumbController;
            _fileDownloadController = fileDownloadController;
            _imageViewerController = imageViewerController;

            _isGroupChat = false;
            _peerDisplayName = string.Empty;
        }

        #endregion

        #region ====== BIND ======

        /// <summary>
        /// (Cách cũ) Bind container FlowLayoutPanel.
        /// </summary>
        public void BindContainer(FlowLayoutPanel container)
        {
            _container = container;
            _renderRoot = container;
        }

        /// <summary>
        /// (Cách mới) Bind root control để Controller có thể update bubble đã render.
        /// - Nếu bạn render trực tiếp vào pnlKhungChat (Panel), hãy gọi BindRenderRoot(pnlKhungChat).
        /// - Nếu bạn dùng row panel thì cũng OK, vì hàm update sẽ duyệt đệ quy.
        /// </summary>
        public void BindRenderRoot(Control renderRoot)
        {
            _renderRoot = renderRoot;
        }

        /// <summary>
        /// SessionController gọi mỗi lần đổi cuộc trò chuyện.
        /// </summary>
        public void SetContext(bool isGroupChat, string peerDisplayName)
        {
            _isGroupChat = isGroupChat;
            _peerDisplayName = peerDisplayName ?? string.Empty;
        }

        #endregion

        #region ====== AVATAR PLACEHOLDER ======

        public Image GetAvatarPlaceholder()
        {
            if (_avatarPlaceholder != null) return _avatarPlaceholder;

            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.LightGray);
                using (Pen p = new Pen(Color.Gray))
                {
                    g.DrawEllipse(p, 2, 2, 60, 60);
                }
            }

            _avatarPlaceholder = bmp;
            return _avatarPlaceholder;
        }

        #endregion

        #region ====== CORE: ADD / CREATE BUBBLE ======

        /// <summary>
        /// Callback cho ChatRenderer (cách cũ): tạo bubble và add vào FlowLayoutPanel container.
        /// </summary>
        public void AddMessageBubbleToContainer(ChatMessage msg)
        {
            if (msg == null) return;

            // FIX cross-thread: ChatRenderer có thể gọi callback từ thread nền.
            // Bắt buộc tạo + add control trên UI thread.
            if (_uiOwner != null && !_uiOwner.IsDisposed && _uiOwner.InvokeRequired)
            {
                try
                {
                    _uiOwner.BeginInvoke((Action)delegate
                    {
                        AddMessageBubbleToContainer(msg);
                    });
                }
                catch
                {
                    // ignore
                }
                return;
            }

            AddMessageBubbleToContainerCore(msg);
        }

        /// <summary>
        /// Factory cho ChatRenderer (cách mới): TẠO bubble control, KHÔNG tự add vào container.
        /// Renderer/ViewController sẽ tự add + layout.
        /// </summary>
        public Control CreateBubbleControl(ChatMessage msg)
        {
            if (msg == null) return null;

            bool isMine = msg.IsMine || string.Equals(msg.SenderId, _currentUserId, StringComparison.Ordinal);

            string displayName = ResolveDisplayName(msg, isMine);
            string time = ChatTextFormatter.FormatTimestamp(msg.Timestamp);

            bool laTinFile = string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase);
            bool laTinAnh = string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase);

            MessageBubbles bubble = new MessageBubbles();
            bubble.Margin = new Padding(3);
            bubble.Tag = msg;

            if (laTinAnh)
            {
                Image placeholder = _imageThumbController.GetOrCreatePlaceholder();
                bubble.SetImageMessage(displayName, placeholder, string.Empty, time, isMine);

                bubble.Cursor = Cursors.Hand;
                AttachImageClickRecursive(bubble);

                // load thumb async
                _imageThumbController.EnsureThumbLoadedAsync(
                    msg,
                    _uiOwner,
                    delegate (Image thumb)
                    {
                        if (thumb == null) return;
                        if (_uiOwner == null || _uiOwner.IsDisposed) return;
                        if (bubble.IsDisposed) return;

                        bubble.SetImageMessage(displayName, thumb, string.Empty, time, isMine);
                        bubble.Cursor = Cursors.Hand;
                        AttachImageClickRecursive(bubble);
                    },
                    320,
                    220);
            }
            else
            {
                string message;
                if (laTinFile)
                {
                    string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                    message = ten + " (" + ChatTextFormatter.FormatBytes(msg.FileSize) + ")";
                }
                else
                {
                    message = msg.Text ?? string.Empty;
                }

                bubble.SetMessage(displayName, message, time, isMine);

                if (laTinFile)
                {
                    bubble.Cursor = Cursors.Hand;
                    AttachFileClickRecursive(bubble);
                }
            }

            return bubble;
        }

        private void AddMessageBubbleToContainerCore(ChatMessage msg)
        {
            if (msg == null) return;
            if (_container == null || _container.IsDisposed) return;

            Control bubble = CreateBubbleControl(msg);
            if (bubble == null) return;

            _container.Controls.Add(bubble);
        }

        #endregion

        #region ====== DISPLAY NAME ======

        private string ResolveDisplayName(ChatMessage msg, bool isMine)
        {
            if (isMine) return "Bạn";

            if (_isGroupChat)
            {
                string name = _groupSenderNameController.GetDisplayName(msg.SenderId);

                // best-effort load display name, rồi update lại các bubble của sender đó
                _groupSenderNameController.EnsureLoadedAsync(
                    msg.SenderId,
                    delegate (string senderId)
                    {
                        return NormalizeSenderNameBlocking(senderId);
                    },
                    _uiOwner,
                    delegate (string senderId)
                    {
                        if (_isGroupChat) UpdateRenderedBubblesSenderName(senderId);
                    });

                return name;
            }

            return _peerDisplayName ?? string.Empty;
        }

        private string NormalizeSenderNameBlocking(string senderId)
        {
            try
            {
                Task<User> t = _authService.GetUserByIdAsync(senderId);
                t.Wait();
                return ChatTextFormatter.FormatUserFullName(t.Result);
            }
            catch
            {
                return senderId;
            }
        }

        public void UpdateRenderedBubblesSenderName(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;

            Control root = _renderRoot;
            if (root == null) root = _container;
            if (root == null || root.IsDisposed) return;

            string newName = _groupSenderNameController.GetDisplayName(senderId);

            try
            {
                root.SuspendLayout();

                foreach (MessageBubbles bubble in EnumerateBubbles(root))
                {
                    if (bubble == null || bubble.IsDisposed) continue;

                    ChatMessage msg = bubble.Tag as ChatMessage;
                    if (msg == null) continue;

                    bool isMine = msg.IsMine || string.Equals(msg.SenderId, _currentUserId, StringComparison.Ordinal);
                    if (isMine) continue;

                    if (!string.Equals(msg.SenderId, senderId, StringComparison.Ordinal)) continue;

                    string time = ChatTextFormatter.FormatTimestamp(msg.Timestamp);

                    bool laTinFile = string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase);
                    bool laTinAnh = string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase);

                    if (laTinAnh)
                    {
                        Image thumb = _imageThumbController.TryGet(msg.MessageId);
                        if (thumb == null) thumb = _imageThumbController.GetOrCreatePlaceholder();

                        bubble.SetImageMessage(newName, thumb, string.Empty, time, false);
                        bubble.Cursor = Cursors.Hand;
                        AttachImageClickRecursive(bubble);
                    }
                    else
                    {
                        string message;
                        if (laTinFile)
                        {
                            string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                            message = ten + " (" + ChatTextFormatter.FormatBytes(msg.FileSize) + ")";
                        }
                        else
                        {
                            message = msg.Text ?? string.Empty;
                        }

                        bubble.SetMessage(newName, message, time, false);

                        if (laTinFile)
                        {
                            bubble.Cursor = Cursors.Hand;
                            AttachFileClickRecursive(bubble);
                        }
                    }
                }
            }
            finally
            {
                try { root.ResumeLayout(); } catch { }
            }
        }

        private static IEnumerable<MessageBubbles> EnumerateBubbles(Control root)
        {
            if (root == null) yield break;

            var stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Control c = stack.Pop();
                if (c == null) continue;

                MessageBubbles b = c as MessageBubbles;
                if (b != null)
                {
                    yield return b;
                }

                // Duyệt con
                Control.ControlCollection children = c.Controls;
                if (children == null) continue;

                for (int i = 0; i < children.Count; i++)
                {
                    stack.Push(children[i]);
                }
            }
        }

        #endregion

        #region ====== CLICK: FILE ======

        private void AttachFileClickRecursive(Control root)
        {
            if (root == null) return;

            root.Click -= BubbleFile_Click;
            root.Click += BubbleFile_Click;

            foreach (Control child in root.Controls)
            {
                AttachFileClickRecursive(child);
            }
        }

        private async void BubbleFile_Click(object sender, EventArgs e)
        {
            ChatMessage msg = FindChatMessageFromTag(sender as Control);
            if (msg == null)
            {
                MessageBox.Show(_uiOwner, "Không lấy được dữ liệu file (Tag bị thiếu).", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await _fileDownloadController.DownloadFromMessageAsync(msg, _uiOwner);
        }

        #endregion

        #region ====== CLICK: IMAGE ======

        private void AttachImageClickRecursive(Control root)
        {
            if (root == null) return;

            root.Click -= BubbleImage_Click;
            root.Click += BubbleImage_Click;

            foreach (Control child in root.Controls)
            {
                AttachImageClickRecursive(child);
            }
        }

        private async void BubbleImage_Click(object sender, EventArgs e)
        {
            ChatMessage msg = FindChatMessageFromTag(sender as Control);
            if (msg == null) return;

            await _imageViewerController.ShowFromMessageAsync(msg, _uiOwner);
        }

        #endregion

        #region ====== TAG HELPER ======

        private ChatMessage FindChatMessageFromTag(Control start)
        {
            Control c = start;
            while (c != null)
            {
                ChatMessage msg = c.Tag as ChatMessage;
                if (msg != null) return msg;
                c = c.Parent;
            }
            return null;
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            try
            {
                if (_avatarPlaceholder != null)
                {
                    _avatarPlaceholder.Dispose();
                    _avatarPlaceholder = null;
                }
            }
            catch { }
        }

        #endregion
    }
}