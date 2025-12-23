using ChatApp.Controllers;
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

        private readonly AuthService _authService = new AuthService();
        private readonly AvatarController _avatarController = new AvatarController();

        private string _lastAvatarKey;

        /// <summary>
        /// Sự kiện khi người dùng click vào item
        /// </summary>
        public event EventHandler ItemClicked;

        #endregion

        public Conversations()
        {
            InitializeComponent();
            lblDisplayName.Click += OnConversationsClick;
            pnlBackground.Click += OnConversationsClick;
            picAvatar.Click += OnConversationsClick;
        }

        public void SetInfo(string fullName, string userId)
        {
            lblDisplayName.Text = fullName;
            UserId = userId;

            Image placeholder = CreateAvatarPlaceholder();
            _ = _avatarController.LoadAvatarToPictureBoxAsync(userId, picAvatar, placeholder);
        }

        private Image CreateAvatarPlaceholder()
        {
            Bitmap bmp = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (Pen p = new Pen(Color.Gray))
                {
                    g.DrawEllipse(p, 1, 1, 46, 46);
                }
            }
            return bmp;
        }

        private void OnConversationsClick(object sender, EventArgs e)
        {
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyTheme(bool isDark)
        {
            // TODO: đổi màu nền/chữ theo isDark
            // ví dụ:
            this.BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.White;
            this.ForeColor = isDark ? Color.White : Color.Black;
            this.Invalidate();
        }


    }
}
