using ChatApp.Controllers;
using ChatApp.Forms;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Net;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Form Nhắn tin (thin view):
    /// - Chỉ làm nhiệm vụ: hook event + gọi Controller
    /// - Mọi logic "làm việc" (listen, render bubble, send file, download...) nằm ở Controllers
    /// </summary>
    public partial class NhanTin : Form
    {
        #region ====== CORE FIELDS ======

        private readonly string _idDangNhap;
        private readonly string _tokenDangNhap;

        #endregion

        #region ====== SERVICES ======

        private readonly ThemeService _themeService = new ThemeService();
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ====== DOMAIN CONTROLLERS ======

        private readonly NhanTinController _nhanTinController;
        private readonly NhanTinNhomController _nhanTinNhomController;

        #endregion

        #region ====== UI CONTROLLERS ======

        private const string GROUP_TAG_PREFIX = "GROUP:";

        private readonly AvatarController _avatarController;
        private readonly ConversationListController _conversationListController;
        private readonly GroupSenderNameController _groupSenderNameController;
        private readonly ImageThumbController _imageThumbController;

        private readonly FileDownloadController _fileDownloadController;
        private readonly ImageViewerController _imageViewerController;

        private readonly ChatViewController _chatViewController;
        private readonly ChatBubbleController _chatBubbleController;

        private readonly ChatSessionController _chatSessionController;
        private readonly ChatStartupController _chatStartupController;
        private readonly GroupCreateController _groupCreateController;

        #endregion

        #region ====== CTOR ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            _idDangNhap = localId;
            _tokenDangNhap = token;

            // TLS 1.2 để tải HTTPS ổn định hơn
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;

            _nhanTinController = new NhanTinController(localId, token);
            _nhanTinNhomController = new NhanTinNhomController(localId, token);

            _avatarController = new AvatarController();
            _groupSenderNameController = new GroupSenderNameController(_authService);
            _imageThumbController = new ImageThumbController();

            _fileDownloadController = new FileDownloadController();
            _imageViewerController = new ImageViewerController();

            // Bubble controller sẽ là nơi tạo MessageBubbles + hook click
            _chatBubbleController = new ChatBubbleController(
                currentUserId: _idDangNhap,
                uiOwner: this,
                authService: _authService,
                groupSenderNameController: _groupSenderNameController,
                imageThumbController: _imageThumbController,
                fileDownloadController: _fileDownloadController,
                imageViewerController: _imageViewerController
            );

            // Chat view controller sẽ wrap ChatRenderer (render trực tiếp lên pnlKhungChat, KHÔNG dùng FlowLayoutPanel)
            _chatViewController = new ChatViewController(
                hostPanel: pnlKhungChat,
                bubbleFactory: _chatBubbleController.CreateBubbleControl
            );

            // init renderer + bind root để controller có thể update bubble đã render (tên người gửi nhóm, ...)
            _chatViewController.Initialize();
            _chatBubbleController.BindRenderRoot(pnlKhungChat);

            // Session controller: quản lý "đang chat ai/nhóm", start/stop listen, gọi render
            _chatSessionController = new ChatSessionController(
                currentUserId: _idDangNhap,
                token: _tokenDangNhap,
                uiOwner: this,
                lblTitle: lblTenDangNhapGiua,
                lblStatus: lblTrangThai,
                pbAvatar: picAnhDaiDienGiua,
                getAvatarPlaceholder: _chatBubbleController.GetAvatarPlaceholder,
                avatarController: _avatarController,
                authService: _authService,
                directMessageController: _nhanTinController,
                groupMessageController: _nhanTinNhomController,
                chatViewController: _chatViewController,
                chatBubbleController: _chatBubbleController
            );

            // Danh sách conversation: click item sẽ chuyển cuộc trò chuyện qua SessionController
            _conversationListController = new ConversationListController(
                pnlDanhSachChat,
                loadFriendsAsync: delegate { return _nhanTinController.GetFriendUsersAsync(_idDangNhap); },
                loadGroupsAsync: delegate { return _nhanTinNhomController.GetMyGroupsAsync(); },
                onItemClicked: delegate (object sender, EventArgs e)
                {
                    if (_chatSessionController != null) _chatSessionController.OnConversationItemClicked(sender, e, GROUP_TAG_PREFIX, _conversationListController);
                },
                groupTagPrefix: GROUP_TAG_PREFIX
            );

            // Startup: reload list + apply theme + set tên mình
            _chatStartupController = new ChatStartupController(
                currentUserId: _idDangNhap,
                themeService: _themeService,
                authService: _authService,
                conversationListController: _conversationListController,
                applyTheme: delegate (bool isDark) { ThemeManager.ApplyTheme(this, isDark); },
                setMeName: delegate (string name) { if (!string.IsNullOrWhiteSpace(name)) lblTenNguoiDung.Text = name; }
            );

            // Tạo nhóm + reload list + open group
            _groupCreateController = new GroupCreateController(
                currentUserId: _idDangNhap,
                token: _tokenDangNhap,
                groupController: _nhanTinNhomController,
                conversationListController: _conversationListController,
                openGroupById: delegate (string groupId)
                {
                    if (_chatSessionController != null) _chatSessionController.OpenGroupConversationById(groupId, _conversationListController);
                }
            );

            HookEvents();
        }

        #endregion

        #region ====== EVENTS HOOK ======

        private void HookEvents()
        {
            Load += NhanTin_Load;
            FormClosed += NhanTin_FormClosed;

            btnGui.Click += btnGui_Click;
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;

            PicSendFile.Click -= PicSendFile_Click;
            PicSendFile.Click += PicSendFile_Click;

            if (btnTaoNhom != null)
            {
                btnTaoNhom.Click -= btnTaoNhom_Click;
                btnTaoNhom.Click += btnTaoNhom_Click;
            }
        }

        #endregion

        #region ====== LIFECYCLE ======

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            await _chatStartupController.InitializeAsync();
        }

        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { if (_chatSessionController != null) _chatSessionController.Dispose(); } catch { }

            try { if (_chatViewController != null) _chatViewController.Dispose(); } catch { }
            try { if (_chatBubbleController != null) _chatBubbleController.Dispose(); } catch { }

            try { _nhanTinController.Dispose(); } catch { }
            try { _nhanTinNhomController.Dispose(); } catch { }

            try { _conversationListController.Dispose(); } catch { }
            try { _groupSenderNameController.Dispose(); } catch { }
            try { _imageThumbController.Dispose(); } catch { }
            try { _fileDownloadController.Dispose(); } catch { }
        }

        #endregion

        #region ====== INPUT / SEND ======

        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnGui.PerformClick();
            }
        }

        private async void btnGui_Click(object sender, EventArgs e)
        {
            string text = (txtNhapTinNhan.Text ?? string.Empty).Trim();
            txtNhapTinNhan.Clear();

            await _chatSessionController.SendTextAsync(text);
        }

        private async void PicSendFile_Click(object sender, EventArgs e)
        {
            await _chatSessionController.PickAndSendAttachmentAsync(this);
        }

        #endregion

        #region ====== RIGHT PANEL BUTTONS ======

        private void btnTaoNhom_Click(object sender, EventArgs e)
        {
            // Dùng controller để mở form tạo nhóm + xử lý create/reload/open
            _groupCreateController.ShowCreateGroupFlow(this, _conversationListController.Friends);
        }

        private void btnSearchFriends_Click(object sender, EventArgs e)
        {
            TimKiemBanBe f = new TimKiemBanBe(_idDangNhap, _tokenDangNhap);
            f.Show();
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            LoiMoiKetBan f = new LoiMoiKetBan(_idDangNhap, _tokenDangNhap);
            f.Show();
        }

        private void picEmoji_Click(object sender, EventArgs e)
        {
            Emoji frm = new Emoji();
            frm.OnEmojiSelected = delegate (string emojiCode)
            {
                txtNhapTinNhan.AppendText(" :" + emojiCode + ": ");
                txtNhapTinNhan.Focus();
                txtNhapTinNhan.SelectionStart = txtNhapTinNhan.Text.Length;
            };

            var pt = picEmoji.PointToScreen(System.Drawing.Point.Empty);
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new System.Drawing.Point(pt.X - (frm.Width / 2) + (picEmoji.Width / 2), pt.Y - frm.Height - 10);
            frm.Show();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Hide();
            NhanTin f = new NhanTin(_idDangNhap, _tokenDangNhap);
            f.TopMost = true;
            f.Show();
            Close();
        }

        #endregion
    }
}
