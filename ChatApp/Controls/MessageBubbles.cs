using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    /// <summary>
    /// Control hiển thị một tin nhắn dạng bubble (chat message)
    /// </summary>
    public partial class MessageBubbles : UserControl
    {
        #region ======= FIELDS =======

        /// <summary>
        /// Xác định tin nhắn có phải của người dùng hiện tại hay không
        /// </summary>
        private bool IsMine;

        #endregion

        #region ======= CONSTRUCTOR =======

        public MessageBubbles()
        {
            InitializeComponent();
        }

        #endregion

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// Gán dữ liệu cho bubble tin nhắn
        /// </summary>
        /// <param name="displayName">Tên người gửi</param>
        /// <param name="message">Nội dung tin nhắn</param>
        /// <param name="time">Thời gian gửi</param>
        /// <param name="isMine">true nếu là tin của mình</param>
        public void SetMessage(
            string displayName,
            string message,
            string time,
            bool isMine)
        {
            lblDisplayName.Text = displayName;
            lblMessage.Text = message;
            lblTime.Text = time;

            IsMine = isMine;

            // Căn vị trí trái / phải
            ApplyLayout(isMine);

            // Áp dụng theme hiện tại
            ApplyTheme(ThemeManager.IsDark);
        }

        /// <summary>
        /// Áp dụng màu sắc theo Light / Dark Mode
        /// </summary>
        public void ApplyTheme(bool isDark)
        {
            #region ===== Container =====

            // Nền ngoài trong suốt
            pnlBackGround.BackColor = Color.Transparent;

            #endregion

            #region ===== Bubble Shape =====

            // Bo góc và padding bubble
            pnlBubble.BorderRadius = 14;
            pnlBubble.Padding = new Padding(4);

            // Đổ bóng bubble
            pnlBubble.ShadowDecoration.Enabled = true;
            pnlBubble.ShadowDecoration.Depth = 4;

            #endregion

            #region ===== Dark Mode =====

            if (isDark)
            {
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#94A3B8");
                lblTime.ForeColor = ColorTranslator.FromHtml("#64748B");

                if (IsMine)
                {
                    // 🌙 Dark – tin nhắn của mình
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#1E3A8A");
                    lblMessage.ForeColor = Color.White;
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#2563EB");
                }
                else
                {
                    // 🌙 Dark – tin nhắn người khác
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#020617");
                    lblMessage.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#0F172A");
                }
            }

            #endregion

            #region ===== Light Mode =====

            else
            {
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#334155");
                lblTime.ForeColor = ColorTranslator.FromHtml("#64748B");

                if (IsMine)
                {
                    // ☀ Light – tin nhắn của mình
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#DBEAFE");
                    lblMessage.ForeColor = ColorTranslator.FromHtml("#0F172A");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#93C5FD");
                }
                else
                {
                    // ☀ Light – tin nhắn người khác
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#F8FAFC");
                    lblMessage.ForeColor = ColorTranslator.FromHtml("#0F172A");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#CBD5F5");
                }
            }

            #endregion
        }

        #endregion

        #region ======= PRIVATE METHODS =======

        /// <summary>
        /// Căn vị trí bubble trái hoặc phải theo người gửi
        /// </summary>
        private void ApplyLayout(bool isMine)
        {
            if (isMine)
            {
                // Tin của mình: căn phải
                this.Dock = DockStyle.Right;
                pnlBackGround.Dock = DockStyle.Right;
                flpBubble.FlowDirection = FlowDirection.TopDown;

                pnlAvatar.Dock = DockStyle.Right;
                flpBubble.Dock = DockStyle.Right;
                pnlBubble.Dock = DockStyle.Right;
                lblDisplayName.Dock = DockStyle.Right;
                lblTime.Dock = DockStyle.Right;
            }
            else
            {
                // Tin người khác: căn trái
                pnlBackGround.Dock = DockStyle.Left;
                flpBubble.FlowDirection = FlowDirection.TopDown;
            }
        }

        #endregion

        #region ======= LIFECYCLE =======

        /// <summary>
        /// Khi control được tạo handle → áp dụng theme hiện tại
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
