using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace ChatApp.Controls
{
    public partial class MessageBubbles : UserControl
    {
        #region ======= FIELDS =======

        private bool IsMine;

        #endregion

        #region ======= CONSTRUCTOR =======

        public MessageBubbles()
        {
            InitializeComponent();
        }

        #endregion

        #region ======= PUBLIC METHODS =======

        public void SetMessage(string displayName, string message, string time, bool isMine)
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
        }

        #endregion

        #region ======= RENDER LOGIC =======

        private void RenderContent(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            // Regex chuẩn để tách mà không mất dữ liệu
            string pattern = @"(:[a-zA-Z0-9_]+:)";
            string[] parts = Regex.Split(message, pattern);

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (Regex.IsMatch(part, pattern))
                {
                    string emojiName = part.Trim(':');
                    string path = Path.Combine(Application.StartupPath, "Resources", "Emoji", emojiName + ".png");

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
                    else { AddTextControl(part); }
                }
                else
                {
                    // Đây là nơi xử lý văn bản thường
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
                AutoSize = true, // Bắt buộc để hiện text dài
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 3, 0, 0)
            };

            // Đảm bảo màu chữ luôn tương phản với nền
            if (ThemeManager.IsDark)
            {
                lbl.ForeColor = IsMine ? Color.White : Color.FromArgb(229, 231, 235);
            }
            else
            {
                lbl.ForeColor = Color.FromArgb(15, 23, 42); // Màu đen đậm dễ nhìn
            }

            flpMessageContent.Controls.Add(lbl);
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
            if (!DesignMode) ApplyTheme(ThemeManager.IsDark);
        }

        #endregion
    }
}