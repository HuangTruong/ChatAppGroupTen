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
    /// NhanTin (thin view): Form nhắn tin “mỏng”.
    /// - Form chỉ lo: khởi tạo, gắn sự kiện, gọi controller
    /// - Toàn bộ logic làm việc (listen realtime, render chat, gửi file/tin, tải file, mở ảnh...)
    ///   đã được tách ra Controllers/Services để dễ bảo trì và mở rộng.
    /// </summary>
    public partial class NhanTin : Form
    {
        #region ====== THÔNG TIN ĐĂNG NHẬP ======

        private readonly string _idDangNhap;
        private readonly string _tokenDangNhap;

        #endregion

        #region ====== SERVICES (NỀN) ======

        private readonly ThemeService _themeService = new ThemeService();
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ====== CONTROLLERS XỬ LÝ DỮ LIỆU ======

        private readonly NhanTinController _nhanTinController;
        private readonly NhanTinNhomController _nhanTinNhomController;

        #endregion

        #region ====== CONTROLLERS/UI: RENDER & TƯƠNG TÁC ======

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

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            _idDangNhap = localId;
            _tokenDangNhap = token;

            SetupNetworkDefaults();

            // Controller dữ liệu
            _nhanTinController = new NhanTinController(_idDangNhap, _tokenDangNhap);
            _nhanTinNhomController = new NhanTinNhomController(_idDangNhap, _tokenDangNhap);

            // Controller UI hỗ trợ
            _avatarController = new AvatarController();
            _groupSenderNameController = new GroupSenderNameController(_authService);
            _imageThumbController = new ImageThumbController();

            _fileDownloadController = new FileDownloadController();
            _imageViewerController = new ImageViewerController();

            // Bubble controller: tạo MessageBubbles + gắn click (file/ảnh) + cập nhật tên sender nhóm
            _chatBubbleController = new ChatBubbleController(
                currentUserId: _idDangNhap,
                uiOwner: this,
                authService: _authService,
                groupSenderNameController: _groupSenderNameController,
                imageThumbController: _imageThumbController,
                fileDownloadController: _fileDownloadController,
                imageViewerController: _imageViewerController
            );

            // Chat view controller: wrap ChatRenderer (render trực tiếp lên pnlKhungChat)
            _chatViewController = new ChatViewController(
                hostPanel: pnlKhungChat,
                bubbleFactory: _chatBubbleController.CreateBubbleControl
            );

            _chatViewController.Initialize();
            _chatBubbleController.BindRenderRoot(pnlKhungChat);

            // Session controller: quản lý “đang chat ai/nhóm”, start/stop listen, render initial/append
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

            // Conversation list: click item sẽ gọi SessionController để mở hội thoại
            _conversationListController = new ConversationListController(
                pnlDanhSachChat,
                loadFriendsAsync: delegate { return _nhanTinController.GetFriendUsersAsync(_idDangNhap); },
                loadGroupsAsync: delegate { return _nhanTinNhomController.GetMyGroupsAsync(); },
                onItemClicked: delegate (object sender, EventArgs e)
                {
                    if (_chatSessionController != null)
                    {
                        _chatSessionController.OnConversationItemClicked(sender, e, GROUP_TAG_PREFIX, _conversationListController);
                    }
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
                setMeName: delegate (string name)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        lblTenNguoiDung.Text = name;
                    }
                }
            );

            // Create group: mở form tạo nhóm + create/reload/open group mới
            _groupCreateController = new GroupCreateController(
                currentUserId: _idDangNhap,
                token: _tokenDangNhap,
                groupController: _nhanTinNhomController,
                conversationListController: _conversationListController,
                openGroupById: delegate (string groupId)
                {
                    if (_chatSessionController != null)
                    {
                        _chatSessionController.OpenGroupConversationById(groupId, _conversationListController);
                    }
                }
            );

            HookEvents();
        }

        private static void SetupNetworkDefaults()
        {
            // TLS 1.2 để HTTPS ổn định hơn (tải file / gọi REST)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
        }

        #endregion

        #region ====== GẮN SỰ KIỆN ======

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

        #region ====== VÒNG ĐỜI FORM ======

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            await _chatStartupController.InitializeAsync().ConfigureAwait(true);
        }

        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            SafeDisposeAll();
        }

        private void SafeDisposeAll()
        {
            try { if (_chatSessionController != null) _chatSessionController.Dispose(); } catch { }

            try { if (_chatViewController != null) _chatViewController.Dispose(); } catch { }
            try { if (_chatBubbleController != null) _chatBubbleController.Dispose(); } catch { }

            try { if (_nhanTinController != null) _nhanTinController.Dispose(); } catch { }
            try { if (_nhanTinNhomController != null) _nhanTinNhomController.Dispose(); } catch { }

            try { if (_conversationListController != null) _conversationListController.Dispose(); } catch { }
            try { if (_groupSenderNameController != null) _groupSenderNameController.Dispose(); } catch { }
            try { if (_imageThumbController != null) _imageThumbController.Dispose(); } catch { }
            try { if (_fileDownloadController != null) _fileDownloadController.Dispose(); } catch { }
        }

        #endregion

        #region ====== NHẬP & GỬI TIN NHẮN ======

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

            await _chatSessionController.SendTextAsync(text).ConfigureAwait(true);
        }

        private async void PicSendFile_Click(object sender, EventArgs e)
        {
            await _chatSessionController.PickAndSendAttachmentAsync(this).ConfigureAwait(true);
        }

        #endregion

        #region ====== CÁC NÚT CHỨC NĂNG ======

        private void btnTaoNhom_Click(object sender, EventArgs e)
        {
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
            frm.Location = new System.Drawing.Point(
                pt.X - (frm.Width / 2) + (picEmoji.Width / 2),
                pt.Y - frm.Height - 10
            );

            frm.Show();
        }

        #endregion
    }
}
