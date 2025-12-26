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
        /// Dịch vụ Auth làm việc với Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ======= CONSTRUCTOR =======

        public Conversations()
        {
            InitializeComponent();

            // Bắt sự kiện click cho toàn bộ item
            lblDisplayName.Click += OnConversationsClick;
            pnlBackground.Click += OnConversationsClick;
            picAvatar.Click += OnConversationsClick;
        }

        #endregion

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// Gán thông tin hiển thị cho item cuộc trò chuyện
        /// </summary>
        /// <param name="fullName">Tên hiển thị</param>
        /// <param name="userId">Id người dùng</param>
        public async void SetInfo(string fullName, string userId)
        {
            lblDisplayName.Text = fullName;
            UserId = userId;

            // Load avatar người dùng (Firebase)
            string base64 = await _authService.GetAvatarAsync(UserId);
            picAvatar.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;
        }

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
