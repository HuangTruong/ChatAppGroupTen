using ChatApp.Helpers;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    /// <summary>
    /// Control hiển thị một cuộc trò chuyện trong danh sách chat
    /// </summary>
    public partial class Conversations : UserControl
    {
        #region ======= PROPERTIES & EVENTS =======

        /// <summary>
        /// Id người dùng của cuộc trò chuyện
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Sự kiện khi người dùng click vào item
        /// </summary>
        public event EventHandler ItemClicked;

        /// <summary>
        /// Sự kiện khi người dùng nhấn nút Hủy lời mời
        /// </summary>
        public event EventHandler CancelClicked;

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        private readonly GroupService _groupService = new GroupService();

        #endregion

        #region ======= GROUP TAG PREFIX =======

        /// <summary>
        /// Prefix tag để nhận biết item nhóm (giống NhanTin.cs).
        /// </summary>
        private const string GROUP_TAG_PREFIX = "GROUP:";

        private string _lastAvatarKey;

        #endregion

        #region ======= CONSTRUCTOR =======

        public Conversations()
        {
            InitializeComponent();

            // Bắt sự kiện click cho toàn bộ item
            lblDisplayName.Click += OnConversationsClick;
            pnlBackground.Click += OnConversationsClick;
            picAvatar.Click += OnConversationsClick;
            picCancelRequest.Click += OnCancelRequestClick;
        }

        #endregion

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// Gán thông tin hiển thị cho item cuộc trò chuyện
        /// </summary>
        /// <param name="DisplayName">Tên hiển thị</param>
        /// <param name="userId">Id người dùng</param>

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// (Backward compatible) - Giữ lại signature cũ.
        /// </summary>
        public void SetInfo(string DisplayName, string userId, bool lanhom)
        {
            // Nếu code cũ truyền lanhom=true thì tự prefix cho đúng format "GROUP:{id}"
            string key = userId ?? string.Empty;
            if (lanhom && !key.StartsWith(GROUP_TAG_PREFIX, StringComparison.Ordinal))
            {
                key = GROUP_TAG_PREFIX + key;
            }

            SetInfo(DisplayName, key);
        }

        /// <summary>
        /// Gán thông tin hiển thị và tự phân biệt avatar user / group bằng prefix "GROUP:".
        /// </summary>
        public async void SetInfo(string DisplayName, string idOrTag)
        {
            lblDisplayName.Text = DisplayName ?? string.Empty;
            UserId = idOrTag;

            if (this.IsDisposed) return;
            if (picAvatar == null || picAvatar.IsDisposed) return;

            // Tránh gọi load avatar trùng nhiều lần
            if (!string.IsNullOrEmpty(_lastAvatarKey) &&
                string.Equals(_lastAvatarKey, idOrTag, StringComparison.Ordinal))
            {
                return;
            }
            _lastAvatarKey = idOrTag;

            try
            {
                string key = idOrTag ?? string.Empty;

                // Giống logic NhanTin.cs: Tag có "GROUP:" thì là nhóm
                if (key.StartsWith(GROUP_TAG_PREFIX, StringComparison.Ordinal))
                {
                    string gid = key.Substring(GROUP_TAG_PREFIX.Length);
                    string base64 = await _groupService.GetAvatarGroupAsync(gid);
                    picAvatar.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;
                }
                else
                {
                    string base64 = await _authService.GetAvatarAsync(key);
                    picAvatar.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;
                }
            }
            catch
            {
                // best-effort
                try { picAvatar.Image = Properties.Resources.DefaultAvatar; } catch { }
            }
        }

        #endregion

        /// <summary>
        /// Áp dụng giao diện Light / Dark cho item
        /// </summary>
        public void ApplyTheme(bool isDark)
        {
            #region ===== Appearance =====

            // Bo góc + viền
            pnlBackground.BorderRadius = 16;
            pnlBackground.BorderThickness = 1;

            // Đổ bóng nhẹ
            pnlBackground.ShadowDecoration.Enabled = true;
            pnlBackground.ShadowDecoration.Depth = 6;

            #endregion

            #region ===== Dark Mode =====

            if (isDark)
            {
                // 🌙 Dark – Night Sky
                pnlBackground.FillColor = ColorTranslator.FromHtml("#020617");
                pnlBackground.BorderColor = ColorTranslator.FromHtml("#1E3A8A");
                pnlBackground.ShadowDecoration.Color = ColorTranslator.FromHtml("#1D4ED8");

                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
            }

            #endregion

            #region ===== Light Mode =====

            else
            {
                // ☀ Light – Sky / Modern Blue
                pnlBackground.FillColor = ColorTranslator.FromHtml("#E0F2FE");
                pnlBackground.BorderColor = ColorTranslator.FromHtml("#BAE6FD");
                pnlBackground.ShadowDecoration.Color = ColorTranslator.FromHtml("#93C5FD");

                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#0F172A");
            }

            #endregion
        }

        #endregion

        #region ======= EVENT HANDLERS =======

        /// <summary>
        /// Xử lý click vào item cuộc trò chuyện
        /// </summary>
        private void OnConversationsClick(object sender, EventArgs e)
        {
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancelRequestClick(object sender, EventArgs e)
        {
            // Ngăn sự kiện click lan ra pnlBackground (nếu cần) và kích hoạt sự kiện Cancel
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region ======= LIFECYCLE =======

        /// <summary>
        /// Khi control được tạo handle → tự động áp dụng theme hiện tại
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode)
            {
                ApplyTheme(ThemeManager.IsDark);
            }
        }

        #endregion
    }
}
