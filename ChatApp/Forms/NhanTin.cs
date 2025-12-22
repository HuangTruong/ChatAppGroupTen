using ChatApp.Controllers;
using ChatApp.Controls;
using ChatApp.Forms;
using ChatApp.Models.Groups;
using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        #region ====== BIẾN THÀNH VIÊN ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string idDangNhap;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string tokenDangNhap;

        /// <summary>
        /// Controller xử lý logic nhắn tin.
        /// </summary>
        private readonly NhanTinController boDieuKhienNhanTin;

        /// <summary>
        /// Controller xử lý logic nhắn tin nhóm.
        /// </summary>
        private readonly NhanTinNhomController boDieuKhienNhanTinNhom;

        /// <summary>
        /// Danh sách nhóm của user (key = groupId).
        /// </summary>
        private Dictionary<string, GroupInfo> tatCaNhom = new Dictionary<string, GroupInfo>();

        /// <summary>
        /// Đang chat nhóm hay 1-1.
        /// </summary>
        private bool dangChatNhom = false;

        /// <summary>
        /// Prefix tag để nhận biết item nhóm trong danh sách chat.
        /// </summary>
        private const string GROUP_TAG_PREFIX = "GROUP:";
        /// <summary>
        /// Danh sách bạn bè (key = safeId).
        /// </summary>
        private Dictionary<string, User> tatCaNguoiDung = new Dictionary<string, User>();

        /// <summary>
        /// localId user đang được chọn để chat.
        /// </summary>
        private string idNguoiDangChat;

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        // ====== CHAT UI CONTAINER (chống lag layout) ======
        private FlowLayoutPanel _chatContainer;

        // ====== BATCH UI (gom nhiều tin mới rồi vẽ 1 lượt) ======
        private readonly object _pendingLock = new object();
        private readonly List<ChatMessage> _pendingAdds = new List<ChatMessage>();
        private System.Windows.Forms.Timer _flushTimer;

        // ====== TRACK TIN ĐÃ VẼ (tránh vẽ trùng) ======
        private readonly HashSet<string> _drawnMessageIds = new HashSet<string>(StringComparer.Ordinal);

        // Giới hạn số bubble giữ trên UI để không nặng dần
        private const int MAX_UI_MESSAGES = 300;

        /// <summary>
        /// Service để lấy thông tin user (FullName) từ Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        /// <summary>
        /// Cache FullName theo userId để hiển thị sender trong nhóm.
        /// </summary>
        private readonly object _senderNameLock = new object();
        private readonly Dictionary<string, string> _senderFullNameCache =
            new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Tránh gọi Firebase trùng lặp khi nhiều tin đến cùng lúc.
        /// </summary>
        private readonly HashSet<string> _senderNameLoading =
            new HashSet<string>(StringComparer.Ordinal);


        #endregion

        #region ====== HÀM KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            idDangNhap = localId;
            tokenDangNhap = token;
            boDieuKhienNhanTin = new NhanTinController(localId, token);


            boDieuKhienNhanTinNhom = new NhanTinNhomController(localId, token);

            // Hook tạo nhóm (Designer có thể chưa gắn event)
            try
            {
                if (btnTaoNhom != null)
                {
                    btnTaoNhom.Click -= btnTaoNhom_Click;
                    btnTaoNhom.Click += btnTaoNhom_Click;
                }
            }
            catch { }

            // Setup chat container bên trong pnlKhungChat
            SetupChatContainer();

            // Event form
            Load += NhanTin_Load;
            FormClosed += NhanTin_FormClosed;

            // Event control
            btnGui.Click += btnGui_Click;
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;

            // ====== SEND FILE: gắn click cho icon gửi file ======
            PicSendFile.Click -= PicSendFile_Click;
            PicSendFile.Click += PicSendFile_Click;
            PicSendFile.Enabled = true;
            PicSendFile.Visible = true;
            PicSendFile.Cursor = Cursors.Hand;
            PicSendFile.BringToFront();

            // ====== SEND FILE: bật TLS 1.2 để tải HTTPS ổn định hơn ======
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;

            // Batch flush timer (UI)
            InitFlushTimer();
        }

        /// <summary>
        /// Tạo FlowLayoutPanel container để vẽ bubble, giảm giật do Dock/SetChildIndex.
        /// </summary>
        private void SetupChatContainer()
        {
            try
            {
                pnlKhungChat.SuspendLayout();
                pnlKhungChat.Controls.Clear();
                pnlKhungChat.AutoScroll = true;

                _chatContainer = new FlowLayoutPanel();
                _chatContainer.Dock = DockStyle.Top;
                _chatContainer.AutoSize = true;
                _chatContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                _chatContainer.WrapContents = false;
                _chatContainer.FlowDirection = FlowDirection.TopDown;
                _chatContainer.Padding = new Padding(6, 6, 6, 6);

                pnlKhungChat.Controls.Add(_chatContainer);

                pnlKhungChat.SizeChanged -= PnlKhungChat_SizeChanged;
                pnlKhungChat.SizeChanged += PnlKhungChat_SizeChanged;
            }
            finally
            {
                pnlKhungChat.ResumeLayout();
            }
        }

        private void PnlKhungChat_SizeChanged(object sender, EventArgs e)
        {
            ResizeChatBubbles();
        }

        private void ResizeChatBubbles()
        {
            if (_chatContainer == null) return;

            int scroll = (pnlKhungChat.VerticalScroll != null && pnlKhungChat.VerticalScroll.Visible)
                ? SystemInformation.VerticalScrollBarWidth
                : 0;

            int w = pnlKhungChat.ClientSize.Width - scroll - 12;
            if (w < 120) w = 120;

            _chatContainer.SuspendLayout();
            try
            {
                foreach (Control c in _chatContainer.Controls)
                {
                    if (c == null) continue;
                    c.Width = w;
                }
            }
            finally
            {
                _chatContainer.ResumeLayout();
            }
        }

        private void InitFlushTimer()
        {
            _flushTimer = new System.Windows.Forms.Timer();
            _flushTimer.Interval = 60; // 50-80ms là ổn: mượt + không spam layout
            _flushTimer.Tick += FlushTimer_Tick;
        }

        #endregion

        #region ====== ENTER ĐỂ GỬI ======

        /// <summary>
        /// Nhấn Enter để gửi, Shift+Enter để xuống dòng.
        /// </summary>
        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnGui.PerformClick();
            }
        }

        #endregion

        #region ====== FORM EVENTS ======

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            await LoadUsersAsync();

            // Load chế độ ngày đêm
            bool isDark = await _themeService.GetThemeAsync(idDangNhap);
            ThemeManager.ApplyTheme(this, isDark);

            // Load trạng thái người dùng offline, online
            if (await _authService.GetStatusAsync(idDangNhap) == "online")
            {
                lblTrangThai.Text = "Online";
            }
            else
            {
                lblTrangThai.Text = "Offline";
            }
        }

        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (_flushTimer != null)
                {
                    _flushTimer.Stop();
                    _flushTimer.Dispose();
                    _flushTimer = null;
                }
            }
            catch { }

            boDieuKhienNhanTin.Dispose();

            try { boDieuKhienNhanTinNhom.Dispose(); } catch { }
        }

        #endregion

        #region ====== LOAD & LỌC DANH SÁCH BẠN BÈ ======

        /// <summary>
        /// Load danh sách bạn bè đã kết bạn từ Firebase:
        /// friends/{me} -> join users/{friendId}.
        /// Hiển thị FullName (ưu tiên) và phụ đề (Email).
        /// </summary>
        private async Task LoadUsersAsync()
        {
            pnlDanhSachChat.SuspendLayout();
            pnlDanhSachChat.Controls.Clear();

            try
            {
                tatCaNguoiDung = await boDieuKhienNhanTin.GetFriendUsersAsync(idDangNhap);



                // Load nhóm và add vào danh sách chat (hiển thị trước bạn bè)
                await LoadGroupsIntoListAsync();
                if (tatCaNguoiDung == null || tatCaNguoiDung.Count == 0)
                {
                    return;
                }

                foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
                {
                    string idNguoiDung = cap.Key; // safeId
                    User nguoiDung = cap.Value;

                    if (nguoiDung != null)
                    {
                        nguoiDung.LocalId = idNguoiDung;
                    }

                    AddUserItem(idNguoiDung, nguoiDung);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách bạn bè: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                pnlDanhSachChat.ResumeLayout();
            }
        }

        /// <summary>
        /// Lọc danh sách bạn bè khi gõ vào ô txtTimKiem.
        /// Tìm theo FullName / DisplayName / Email.
        /// </summary>

        #endregion

        #region ====== ITEM DANH SÁCH BẠN BÈ (UI) ======

        /// <summary>
        /// Tạo 1 item user trong flpDanhSachChat.
        /// </summary>
        private void AddUserItem(string userId, User user)
        {
            Conversations conversations = new Conversations();
            conversations.Cursor = Cursors.Hand;

            conversations.SetInfo(GetUserFullName(user), GetUserSubtitle(user, userId));
            conversations.Tag = userId;

            conversations.ItemClicked -= UserItem_Click;
            conversations.ItemClicked += UserItem_Click;

            conversations.Dock = DockStyle.Top;    
            pnlDanhSachChat.Controls.Add(conversations);
        }

        private static string GetUserFullName(User user)
        {
            if (user == null)
            {
                return "Người dùng";
            }

            string ten = user.FullName;

            if (string.IsNullOrWhiteSpace(ten))
            {
                ten = user.DisplayName;
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                string email = user.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    int at = email.IndexOf('@');
                    ten = (at > 0) ? email.Substring(0, at) : email;
                }
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                return "Người dùng";
            }

            ten = Regex.Replace(ten.Trim(), "\\s+", " ");

            try
            {
                CultureInfo vi = new CultureInfo("vi-VN");
                ten = vi.TextInfo.ToTitleCase(ten.ToLower(vi));
            }
            catch
            {
                // ignore
            }

            return ten;
        }

        private static string GetUserSubtitle(User user, string userId)
        {
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                return user.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId;
            }

            return string.Empty;
        }

        #endregion

        #region ====== GROUP SENDER FULLNAME ======

        /// <summary>
        /// Lấy FullName của sender trong nhóm (ưu tiên cache).
        /// Nếu chưa có cache thì trả về senderId tạm thời.
        /// </summary>
        private string GetGroupSenderDisplayName(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                return "Người dùng";
            }

            lock (_senderNameLock)
            {
                string name;
                if (_senderFullNameCache.TryGetValue(senderId, out name))
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }

            // fallback tạm (sẽ tự update sau khi tải xong)
            return senderId;
        }

        /// <summary>
        /// Đảm bảo đã tải FullName sender từ Firebase (chỉ tải 1 lần).
        /// Khi tải xong sẽ update lại những bubble của sender đó.
        /// </summary>
        private void EnsureGroupSenderFullNameLoaded(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;

            // Nếu không ở chế độ nhóm thì thôi
            if (!dangChatNhom) return;

            // Tránh request trùng
            lock (_senderNameLock)
            {
                if (_senderFullNameCache.ContainsKey(senderId))
                {
                    return;
                }

                if (_senderNameLoading.Contains(senderId))
                {
                    return;
                }

                _senderNameLoading.Add(senderId);
            }

            Task.Run(async delegate
            {
                string fullName = null;

                try
                {
                    User u = await _authService.GetUserByIdAsync(senderId).ConfigureAwait(false);
                    fullName = GetUserFullName(u);
                }
                catch
                {
                    fullName = null;
                }

                lock (_senderNameLock)
                {
                    _senderNameLoading.Remove(senderId);

                    // set cache (kể cả null để khỏi gọi lại liên tục)
                    _senderFullNameCache[senderId] = fullName;
                }

                // Update UI nếu vẫn đang chat nhóm đó
                try
                {
                    if (this.IsDisposed) return;

                    this.BeginInvoke((Action)delegate
                    {
                        if (this.IsDisposed) return;
                        if (!dangChatNhom) return;

                        UpdateRenderedBubblesSenderName(senderId);
                    });
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Duyệt các bubble đã render, bubble nào thuộc senderId thì set lại displayName.
        /// </summary>
        private void UpdateRenderedBubblesSenderName(string senderId)
        {
            if (_chatContainer == null) return;
            if (string.IsNullOrWhiteSpace(senderId)) return;

            string newName = GetGroupSenderDisplayName(senderId);

            _chatContainer.SuspendLayout();
            try
            {
                foreach (Control c in _chatContainer.Controls)
                {
                    MessageBubbles bubble = c as MessageBubbles;
                    if (bubble == null) continue;

                    ChatMessage msg = bubble.Tag as ChatMessage;
                    if (msg == null) continue;

                    if (msg.IsMine) continue;

                    if (!string.Equals(msg.SenderId, senderId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool laTinFile = string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase);
                    bool laTinAnh = string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase);

                    string time = FormatTimestamp(msg.Timestamp);

                    if (laTinAnh)
                    {
                        Image thumb = TaoThumbnailTuMessage(msg);
                        // Ảnh trong chat: không hiện tên file (UI gọn hơn)
                        string caption = string.Empty;

                        bubble.SetImageMessage(newName, thumb, caption, time, false);

                        bubble.Cursor = Cursors.Hand;
                        GanClickDeXemAnh(bubble);
                    }
                    else
                    {
                        string message;
                        if (laTinFile)
                        {
                            string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                            message = ten + " (" + FormatBytes(msg.FileSize) + ")";
                        }
                        else
                        {
                            message = msg.Text ?? string.Empty;
                        }

                        bubble.SetMessage(newName, message, time, false);

                        if (laTinFile)
                        {
                            bubble.Cursor = Cursors.Hand;
                            GanClickDeTaiFile(bubble);
                        }
                    }
                }
            }
            finally
            {
                _chatContainer.ResumeLayout();
            }
        }

        #endregion

        #region ====== CHỌN USER & MỞ CUỘC TRÒ CHUYỆN ======

        private void UserItem_Click(object sender, EventArgs e)
        {
            Conversations conversations = sender as Conversations;
            if (conversations == null) return;

            string idNguoiDung = conversations.Tag as string;
            if (string.IsNullOrEmpty(idNguoiDung)) return;

            // Nếu là group item (Tag = "GROUP:{groupId}")
            if (idNguoiDung.StartsWith(GROUP_TAG_PREFIX, StringComparison.Ordinal))
            {
                string gid = idNguoiDung.Substring(GROUP_TAG_PREFIX.Length);
                OpenGroupConversation(gid);
                return;
            }

            OpenConversation(idNguoiDung);
        }

        /// <summary>
        /// Mở cuộc trò chuyện với 1 user:
        /// - Cập nhật tên ở panel giữa/phải.
        /// - Clear UI chat.
        /// - Listen incremental (append tin mới thay vì reload full).
        /// </summary>
        private void OpenConversation(string otherUserId)
        {
            idNguoiDangChat = otherUserId;


            // Chuyển về chế độ chat 1-1
            dangChatNhom = false;

            // Dừng listen nhóm nếu đang chạy
            try { boDieuKhienNhanTinNhom.StopListen(); } catch { }
            User nguoiDung;
            if (tatCaNguoiDung != null && tatCaNguoiDung.TryGetValue(otherUserId, out nguoiDung))
            {
                string ten = GetUserFullName(nguoiDung);
                lblTenDangNhapGiua.Text = ten;
            }
            else
            {
                lblTenDangNhapGiua.Text = "";
            }

            ResetChatUI();

            // Listen incremental (mượt hơn rất nhiều)
            boDieuKhienNhanTin.StartListenConversation(
                otherUserId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    if (!IsHandleCreated) return;
                    try
                    {
                        BeginInvoke(new Action(delegate
                        {
                            RenderInitialMessages(initial, otherUserId);
                        }));
                    }
                    catch { }
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    if (!IsHandleCreated) return;
                    try
                    {
                        BeginInvoke(new Action(delegate
                        {
                            QueueAppendMessage(msg, otherUserId);
                        }));
                    }
                    catch { }
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    if (!IsHandleCreated) return;
                    try
                    {
                        BeginInvoke(new Action(delegate
                        {
                            // Reset hiếm khi xảy ra (edit/remove/snapshot)
                            RenderInitialMessages(full, otherUserId);
                        }));
                    }
                    catch { }
                });
        }

        private void ResetChatUI()
        {
            lock (_pendingLock)
            {
                _pendingAdds.Clear();
            }

            _drawnMessageIds.Clear();

            if (_flushTimer != null)
            {
                _flushTimer.Stop();
            }

            if (_chatContainer != null)
            {
                _chatContainer.SuspendLayout();
                try
                {
                    _chatContainer.Controls.Clear();
                }
                finally
                {
                    _chatContainer.ResumeLayout();
                }
            }
        }

        #endregion

        #region ====== VẼ TIN NHẮN ======

        /// <summary>
        /// Vẽ batch ban đầu (1 lần) - tránh render lại liên tục.
        /// </summary>
        private void RenderInitialMessages(IList<ChatMessage> messages, string ownerUserId)
        {
            if (!string.Equals(idNguoiDangChat, ownerUserId, StringComparison.Ordinal))
            {
                return;
            }

            ResetChatUI();

            if (messages == null || messages.Count == 0)
            {
                return;
            }

            if (_chatContainer == null) return;

            _chatContainer.SuspendLayout();
            try
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    ChatMessage m = messages[i];
                    if (m == null) continue;

                    if (!string.IsNullOrEmpty(m.MessageId))
                    {
                        if (_drawnMessageIds.Contains(m.MessageId))
                        {
                            continue;
                        }
                        _drawnMessageIds.Add(m.MessageId);
                    }

                    AddMessageBubbleToContainer(m);
                }
            }
            finally
            {
                _chatContainer.ResumeLayout();
                ResizeChatBubbles();
                ScrollToBottom();
            }
        }

        /// <summary>
        /// Queue append tin mới (gom lại để UI không giật khi tin đến dồn dập).
        /// </summary>
        private void QueueAppendMessage(ChatMessage msg, string ownerUserId)
        {
            if (!string.Equals(idNguoiDangChat, ownerUserId, StringComparison.Ordinal))
            {
                return;
            }

            if (msg == null) return;

            // Tránh vẽ trùng
            if (!string.IsNullOrEmpty(msg.MessageId))
            {
                if (_drawnMessageIds.Contains(msg.MessageId))
                {
                    return;
                }
            }

            lock (_pendingLock)
            {
                _pendingAdds.Add(msg);
            }

            if (_flushTimer != null && !_flushTimer.Enabled)
            {
                _flushTimer.Start();
            }
        }

        private void FlushTimer_Tick(object sender, EventArgs e)
        {
            if (_chatContainer == null) return;

            List<ChatMessage> batch = null;

            lock (_pendingLock)
            {
                if (_pendingAdds.Count == 0)
                {
                    if (_flushTimer != null) _flushTimer.Stop();
                    return;
                }

                batch = new List<ChatMessage>(_pendingAdds);
                _pendingAdds.Clear();
            }

            // Sort theo time để append đúng thứ tự
            batch.Sort(delegate (ChatMessage a, ChatMessage b)
            {
                long ta = (a != null) ? a.Timestamp : 0;
                long tb = (b != null) ? b.Timestamp : 0;
                if (ta < tb) return -1;
                if (ta > tb) return 1;
                return 0;
            });

            _chatContainer.SuspendLayout();
            try
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    ChatMessage m = batch[i];
                    if (m == null) continue;

                    if (!string.IsNullOrEmpty(m.MessageId))
                    {
                        if (_drawnMessageIds.Contains(m.MessageId))
                        {
                            continue;
                        }
                        _drawnMessageIds.Add(m.MessageId);
                    }

                    AddMessageBubbleToContainer(m);
                }

                TrimOldMessagesIfNeeded();
            }
            finally
            {
                _chatContainer.ResumeLayout();
                ResizeChatBubbles();
                ScrollToBottom();
            }
        }

        private void TrimOldMessagesIfNeeded()
        {
            if (_chatContainer == null) return;

            int extra = _chatContainer.Controls.Count - MAX_UI_MESSAGES;
            if (extra <= 0) return;

            // Xóa bớt bubble cũ nhất ở đầu (TopDown => index 0 là cũ nhất)
            for (int i = 0; i < extra; i++)
            {
                if (_chatContainer.Controls.Count == 0) break;

                Control c = _chatContainer.Controls[0];
                _chatContainer.Controls.RemoveAt(0);

                try { c.Dispose(); } catch { }
            }

            // Note: _drawnMessageIds vẫn giữ, nhưng không sao (tránh vẽ trùng).
            // Nếu bạn muốn tối ưu bộ nhớ thêm, có thể rebuild HashSet theo controls còn lại.
        }

        private void ScrollToBottom()
        {
            if (_chatContainer == null) return;
            if (_chatContainer.Controls.Count == 0) return;

            Control last = _chatContainer.Controls[_chatContainer.Controls.Count - 1];
            try
            {
                pnlKhungChat.ScrollControlIntoView(last);
            }
            catch { }
        }

        /// <summary>
        /// Thêm 1 bong bóng tin nhắn vào container.
        /// </summary>
        private void AddMessageBubbleToContainer(ChatMessage msg)
        {
            if (msg == null) return;

            bool isMine = msg.IsMine;

            string displayName;
            if (isMine)
            {
                displayName = "Bạn";
            }
            else if (dangChatNhom)
            {
                displayName = GetGroupSenderDisplayName(msg.SenderId);
                EnsureGroupSenderFullNameLoaded(msg.SenderId);
            }
            else
            {
                displayName = (lblTenDangNhapGiua.Text ?? string.Empty);
            }

            bool laTinFile = string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase);
            bool laTinAnh = string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase);

            string time = FormatTimestamp(msg.Timestamp);

            MessageBubbles bubble = new MessageBubbles();
            bubble.Margin = new Padding(3, 3, 3, 3);

            bubble.Tag = msg;

            if (laTinAnh)
            {
                Image thumb = TaoThumbnailTuMessage(msg);
                // Ảnh trong chat: không hiện tên file (UI gọn hơn)
                string caption = string.Empty;

                bubble.SetImageMessage(displayName, thumb, caption, time, isMine);

                bubble.Cursor = Cursors.Hand;
                GanClickDeXemAnh(bubble);
            }
            else
            {
                string message;
                if (laTinFile)
                {
                    string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                    message = ten + " (" + FormatBytes(msg.FileSize) + ")";
                }
                else
                {
                    message = msg.Text ?? string.Empty;
                }

                bubble.SetMessage(displayName, message, time, isMine);

                if (laTinFile)
                {
                    bubble.Cursor = Cursors.Hand;
                    GanClickDeTaiFile(bubble);
                }
            }

            _chatContainer.Controls.Add(bubble);

        }

        private static string FormatTimestamp(long timestamp)
        {
            if (timestamp <= 0) return string.Empty;

            try
            {
                DateTime thoiGian = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
                return thoiGian.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region ====== GỬI TIN NHẮN ======
        private async void btnGui_Click(object sender, EventArgs e)
        {
            string noiDungTin = (txtNhapTinNhan.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(noiDungTin))
            {
                return;
            }

            if (string.IsNullOrEmpty(idNguoiDangChat))
            {
                MessageBox.Show(
                    "Vui lòng chọn người hoặc nhóm cần nhắn tin ở danh sách bên trái.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            txtNhapTinNhan.Clear();

            try
            {
                if (dangChatNhom)
                {
                    ChatMessage sent = await boDieuKhienNhanTinNhom.SendGroupMessageAsync(idNguoiDangChat, noiDungTin);
                    if (sent != null)
                    {
                        QueueAppendMessage(sent, idNguoiDangChat);
                    }
                }
                else
                {
                    await boDieuKhienNhanTin.SendMessageAsync(idNguoiDangChat, noiDungTin);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi gửi tin nhắn: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== NHÓM CHAT ======

        /// <summary>
        /// Load danh sách nhóm của user và add vào flpDanhSachChat.
        /// Được gọi bên trong LoadUsersAsync() để hiển thị nhóm trước bạn bè.
        /// </summary>
        private async Task LoadGroupsIntoListAsync()
        {
            try
            {
                tatCaNhom = await boDieuKhienNhanTinNhom.GetMyGroupsAsync();
                if (tatCaNhom == null) tatCaNhom = new Dictionary<string, GroupInfo>();

                List<GroupInfo> list = tatCaNhom.Values.ToList();
                list.Sort(delegate (GroupInfo a, GroupInfo b)
                {
                    long ta = a != null ? a.LastMessageAt : 0;
                    long tb = b != null ? b.LastMessageAt : 0;
                    return tb.CompareTo(ta);
                });

                foreach (GroupInfo g in list)
                {
                    AddGroupItem(g);
                }
            }
            catch
            {
                // ignore (best-effort)
            }
        }

        private void AddGroupItem(GroupInfo g)
        {
            if (g == null) return;

            Conversations item = new Conversations();
            item.ItemClicked += UserItem_Click;

            // Tag dạng "GROUP:{groupId}"
            item.Tag = GROUP_TAG_PREFIX + g.GroupId;

            string title = string.IsNullOrWhiteSpace(g.Name) ? ("Nhóm " + g.GroupId) : g.Name;

            string subtitle = g.LastMessage;
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                subtitle = g.MemberCount > 0 ? (g.MemberCount + " thành viên") : "Nhóm chat";
            }

            item.SetInfo(title, subtitle);
            pnlDanhSachChat.Controls.Add(item);
        }

        /// <summary>
        /// Mở cuộc trò chuyện nhóm.
        /// </summary>
        private void OpenGroupConversation(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            idNguoiDangChat = groupId;
            dangChatNhom = true;

            // Dừng listen 1-1 để tránh append nhầm
            try { boDieuKhienNhanTin.StopListen(); } catch { }

            // Set tiêu đề
            GroupInfo g;
            if (tatCaNhom != null && tatCaNhom.TryGetValue(groupId, out g) && g != null)
            {
                lblTenDangNhapGiua.Text = g.Name;
            }
            else
            {
                lblTenDangNhapGiua.Text = "Nhóm chat";
            }

            ResetChatUI();

            boDieuKhienNhanTinNhom.StartListenGroup(
                groupId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    if (this.IsDisposed) return;

                    this.BeginInvoke((Action)delegate
                    {
                        RenderInitialMessages(initial, groupId);
                    });
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    if (this.IsDisposed) return;

                    this.BeginInvoke((Action)delegate
                    {
                        QueueAppendMessage(msg, groupId);
                    });
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    if (this.IsDisposed) return;

                    this.BeginInvoke((Action)delegate
                    {
                        RenderInitialMessages(full, groupId);
                    });
                });
        }

        private async void btnTaoNhom_Click(object sender, EventArgs e)
        {
            try
            {
                if (tatCaNguoiDung == null || tatCaNguoiDung.Count == 0)
                {
                    MessageBox.Show("Chưa có danh sách bạn bè để tạo nhóm.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (TaoNhom f = new TaoNhom(tatCaNguoiDung,idDangNhap,tokenDangNhap))
                {
                    if (f.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    string groupName = f.GroupName;
                    List<string> members = f.SelectedMemberIds;

                    string newGroupId = await boDieuKhienNhanTinNhom.CreateGroupAsync(groupName, members);

                    // Reload list để thấy nhóm mới
                    await LoadUsersAsync();

                    // Auto open nhóm mới
                    if (!string.IsNullOrWhiteSpace(newGroupId))
                    {
                        OpenGroupConversation(newGroupId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tạo nhóm thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== NÚT SEND FILE ======

        private bool dangGuiFile = false;
        private bool dangTaiFile = false;

        private static readonly HttpClient httpClient = TaoHttpClientTaiFile();

        private static HttpClient TaoHttpClientTaiFile()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseProxy = true;
            handler.Proxy = WebRequest.DefaultWebProxy;

            HttpClient c = new HttpClient(handler);
            c.Timeout = TimeSpan.FromMinutes(5);

            c.DefaultRequestHeaders.UserAgent.Clear();
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            return c;
        }

        private async void PicSendFile_Click(object sender, EventArgs e)
        {
            if (dangGuiFile) return;
            dangGuiFile = true;

            try
            {
                if (string.IsNullOrEmpty(idNguoiDangChat))
                {
                    MessageBox.Show("Chọn người để chat trước đã.");
                    return;
                }

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Chọn file để gửi";
                    ofd.Filter = "All files (*.*)|*.*";

                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    if (dangChatNhom)
                    {
                        ChatMessage sent = await boDieuKhienNhanTinNhom.SendGroupAttachmentMessageAsync(idNguoiDangChat, ofd.FileName);
                        if (sent != null)
                        {
                            QueueAppendMessage(sent, idNguoiDangChat);
                        }
                    }
                    else
                    {
                        await boDieuKhienNhanTin.SendAttachmentMessageAsync(idNguoiDangChat, ofd.FileName);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi file: " + ex.Message);
            }
            finally
            {
                dangGuiFile = false;
            }
        }

        private static string FormatBytes(long bytes)
        {
            double b = bytes;
            string[] u = { "B", "KB", "MB", "GB" };
            int i = 0;

            while (b >= 1024 && i < u.Length - 1)
            {
                b /= 1024;
                i++;
            }

            return string.Format("{0:0.##} {1}", b, u[i]);
        }

        private void GanClickDeTaiFile(Control root)
        {
            if (root == null) return;

            root.Click -= BubbleFile_Click;
            root.Click += BubbleFile_Click;

            foreach (Control child in root.Controls)
            {
                GanClickDeTaiFile(child);
            }
        }

        private ChatMessage LayChatMessageTuTag(Control start)
        {
            Control c = start;
            while (c != null)
            {
                ChatMessage msg = c.Tag as ChatMessage;
                if (msg != null)
                {
                    return msg;
                }
                c = c.Parent;
            }
            return null;
        }

        private async void BubbleFile_Click(object sender, EventArgs e)
        {
            if (dangTaiFile) return;
            dangTaiFile = true;

            try
            {
                ChatMessage msg = LayChatMessageTuTag(sender as Control);

                if (msg == null)
                {
                    MessageBox.Show("Không lấy được dữ liệu file (Tag bị thiếu).");
                    return;
                }

                if (!string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(msg.FileUrl))
                {
                    MessageBox.Show("Tin nhắn file không có URL để tải.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    string downloads = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Downloads");
                    if (Directory.Exists(downloads))
                    {
                        sfd.InitialDirectory = downloads;
                    }

                    sfd.FileName = string.IsNullOrEmpty(msg.FileName) ? "download.bin" : msg.FileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    using (HttpResponseMessage resp =
                        await httpClient.GetAsync(msg.FileUrl.Trim(), HttpCompletionOption.ResponseHeadersRead))
                    {
                        resp.EnsureSuccessStatusCode();

                        using (Stream net = await resp.Content.ReadAsStreamAsync())
                        using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await net.CopyToAsync(fs);
                        }
                    }

                    MessageBox.Show("Tải xong:\n" + sfd.FileName);
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Không ghi được file vì file đang được mở/đang bị khóa.\nĐóng file đó hoặc chọn tên khác rồi tải lại.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tải file lỗi:\n" + ex.ToString());
            }
            finally
            {
                dangTaiFile = false;
            }
        }



        #region ====== ẢNH: CLICK ĐỂ XEM FULL + DOWNLOAD ======

        private void GanClickDeXemAnh(Control root)
        {
            if (root == null) return;

            root.Click -= BubbleImage_Click;
            root.Click += BubbleImage_Click;

            foreach (Control child in root.Controls)
            {
                GanClickDeXemAnh(child);
            }
        }

        private Image TaoThumbnailTuMessage(ChatMessage msg)
        {
            if (msg == null) return null;
            if (string.IsNullOrWhiteSpace(msg.ImageBase64)) return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(msg.ImageBase64);
                // Thumbnail trong chat
                return TaoThumbnailTuBytes(bytes, 320, 220);
            }
            catch
            {
                return null;
            }
        }

        private static Image TaoThumbnailTuBytes(byte[] bytes, int maxW, int maxH)
        {
            if (bytes == null || bytes.Length == 0) return null;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (Image img = Image.FromStream(ms))
            {
                int w = img.Width;
                int h = img.Height;
                if (w <= 0 || h <= 0) return null;

                double scale = Math.Min((double)maxW / w, (double)maxH / h);
                if (scale > 1) scale = 1;

                int tw = Math.Max(1, (int)Math.Round(w * scale));
                int th = Math.Max(1, (int)Math.Round(h * scale));

                Bitmap bmp = new Bitmap(tw, th);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(img, 0, 0, tw, th);
                }
                return bmp;
            }
        }

        private async void BubbleImage_Click(object sender, EventArgs e)
        {
            await Task.Yield();

            ChatMessage msg = LayChatMessageTuTag(sender as Control);
            if (msg == null) return;

            if (!string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(msg.ImageBase64))
            {
                MessageBox.Show("Tin nhắn ảnh bị thiếu dữ liệu (ImageBase64).");
                return;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(msg.ImageBase64);
            }
            catch
            {
                MessageBox.Show("Ảnh bị lỗi/không đọc được (base64 sai).");
                return;
            }

            string fileName = string.IsNullOrWhiteSpace(msg.FileName) ? "image" : msg.FileName;

            using (ImageViewerForm viewer = new ImageViewerForm(bytes, fileName, msg.ImageMimeType))
            {
                viewer.ShowDialog(this);
            }
        }

        #endregion

        #endregion

        #region ====== MỞ FORM KHÁC ======

        private void btnSearchFriends_Click(object sender, EventArgs e)
        {
            TimKiemBanBe timKiemBanBe = new TimKiemBanBe(idDangNhap, tokenDangNhap);
            timKiemBanBe.Show();
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            FormLoiMoiKetBan formLoiMoiKetBan = new FormLoiMoiKetBan(idDangNhap, tokenDangNhap);
            formLoiMoiKetBan.Show();
        }

        #endregion

        #region ====== EMOJI ======

        private void picEmoji_Click(object sender, EventArgs e)
        {
            FormEmoji frm = new FormEmoji();

            // Nhận emoji từ FormEmoji
            frm.OnEmojiSelected = (emojiCode) => {
                // Thêm mã emoji vào văn bản hiện tại
                txtNhapTinNhan.AppendText($" :{emojiCode}: ");

                // Quan trọng: Trả focus về ô nhập liệu để người dùng gõ tiếp được ngay
                txtNhapTinNhan.Focus();
                txtNhapTinNhan.SelectionStart = txtNhapTinNhan.Text.Length;
            };

            // Tính toán vị trí hiển thị (Phía trên nút bấm)
            Point pt = picEmoji.PointToScreen(Point.Empty);
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new Point(pt.X - (frm.Width / 2) + (picEmoji.Width / 2), pt.Y - frm.Height - 10);

            frm.Show();
        }

        #endregion

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.Hide();
            var f = new NhanTin(idDangNhap,tokenDangNhap);
            f.TopMost = true;
            f.Show();
            this.Close();
        }
    }
}