using ChatApp.Helpers;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    public partial class MessageBubbles : UserControl
    {
        #region ======= FIELDS =======

        private bool IsMine;

        /// <summary>
        /// Dịch vụ Auth làm việc với Firebase.
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        #endregion

        #region ======= CONSTRUCTOR =======

        public MessageBubbles()
        {
            InitializeComponent();
        }

        #endregion

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// Hiển thị tin nhắn text (bao gồm emoji). 
        /// Tất cả file/ảnh bên ngoài sẽ được render thành text "TênFile (Size)" từ NhanTin.cs.
        /// </summary>
        public async void SetMessage(string displayName, string message, string time, bool isMine, string senderId)
        {
            lblDisplayName.Text = displayName;
            lblTime.Text = time;
            IsMine = isMine;

            // 1. Căn vị trí trái / phải
            ApplyLayout(isMine);

            // 2. Áp dụng màu sắc cho khung (Bubble)
            ApplyTheme(ThemeManager.IsDark);

            // 3. Xử lý hiển thị nội dung (Chữ + Emoji)
            flpMessageContent.Controls.Clear();
            RenderContent(message);

            // 4. Fix: Bubble cao bao nhiêu thì UserControl cao theo (khỏi bị cắt)
            FitBubbleHeight();

            // 5. Load avatar người dùng (Firebase)
            try
            {
                string base64 = await _authService.GetAvatarAsync(senderId);
                picAvatar.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;
            }
            catch
            {
                picAvatar.Image = Properties.Resources.DefaultAvatar;
            }
        }

        #endregion

        #region ======= RENDER LOGIC =======

        private void RenderContent(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            // Regex chuẩn để tách mà không mất dữ liệu
            string pattern = @"(:[a-zA-Z0-9_]+:)";
            string[] parts = Regex.Split(message, pattern);

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (Regex.IsMatch(part, pattern))
                {
                    string emojiName = part.Trim(':');
                    string path = Path.Combine(
                        Application.StartupPath,
                        "Resources",
                        "Emoji",
                        emojiName + ".png"
                    );

                    if (File.Exists(path))
                    {
                        PictureBox pic = new PictureBox
                        {
                            Image = Image.FromFile(path),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Size = new Size(24, 24),
                            Margin = new Padding(1, 1, 1, 0)
                        };
                        flpMessageContent.Controls.Add(pic);
                    }
                    else
                    {
                        AddTextControl(part);
                    }
                }
                else
                {
                    // Văn bản thường
                    AddTextControl(part);
                }
            }
        }

        private void AddTextControl(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Label lbl = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.Transparent
            };

            // Đảm bảo màu chữ luôn tương phản với nền
            if (ThemeManager.IsDark)
            {
                lbl.ForeColor = IsMine ? Color.White : Color.FromArgb(229, 231, 235);
            }
            else
            {
                lbl.ForeColor = Color.FromArgb(15, 23, 42);
            }

            flpMessageContent.Controls.Add(lbl);
        }

        /// <summary>
        /// Khi nội dung (emoji/text) cao hơn -> bị cắt.
        /// => Lấy PreferredSize của pnlBackGround rồi set Height tương ứng.
        /// </summary>
        private void FitBubbleHeight()
        {
            try
            {
                // ép layout tính lại size
                flpMessageContent.PerformLayout();
                pnlBubble.PerformLayout();
                flpBubble.PerformLayout();
                pnlBackGround.PerformLayout();

                int targetH = pnlBackGround.PreferredSize.Height;
                if (targetH <= 0) targetH = pnlBackGround.Height;

                // chỉ cần tăng height; width giữ nguyên để khỏi phá layout hiện tại
                if (targetH > this.Height)
                {
                    this.Height = targetH;
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ======= THEME & LAYOUT =======

        public void ApplyTheme(bool isDark)
        {
            pnlBackGround.BackColor = Color.Transparent;
            pnlBubble.BorderRadius = 14;
            pnlBubble.ShadowDecoration.Enabled = true;
            pnlBubble.ShadowDecoration.Depth = 4;

            if (isDark)
            {
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#94A3B8");
                lblTime.ForeColor = ColorTranslator.FromHtml("#64748B");

                if (IsMine)
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#1E3A8A");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#2563EB");
                }
                else
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#020617");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#0F172A");
                }
            }
            else
            {
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#334155");
                lblTime.ForeColor = ColorTranslator.FromHtml("#64748B");

                if (IsMine)
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#DBEAFE");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#93C5FD");
                }
                else
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#F8FAFC");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#CBD5F5");
                }
            }
        }

        private void ApplyLayout(bool isMine)
        {
            if (isMine)
            {
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
                this.Dock = DockStyle.Left;
                pnlBackGround.Dock = DockStyle.Left;
                flpBubble.FlowDirection = FlowDirection.TopDown;
                pnlAvatar.Dock = DockStyle.Left;
                flpBubble.Dock = DockStyle.Left;
                pnlBubble.Dock = DockStyle.Left;
                lblDisplayName.Dock = DockStyle.Left;
                lblTime.Dock = DockStyle.Left;
            }
        }

        #endregion

        #region ======= LIFECYCLE =======

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
