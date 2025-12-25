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

        // ===== ẢNH: GIỚI HẠN HIỂN THỊ (tránh ảnh to làm vỡ layout) =====
        private const int IMG_MAX_W = 320;
        private const int IMG_MAX_H = 240;

        // Giới hạn bề ngang chữ để nó tự xuống dòng
        private const int TEXT_MAX_W = 280;

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

            // Load avatar người dùng (Firebase)
            string base64 = await _authService.GetAvatarAsync(senderId);
            picAvatar.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;
        }
            /// <summary>
            /// Hiển thị tin nhắn ảnh (thumbnail + caption).
            /// </summary>
        public void SetImageMessage(string displayName, Image thumbnail, string caption, string time, bool isMine)
        {
            lblDisplayName.Text = displayName;
            lblTime.Text = time;
            IsMine = isMine;

            ApplyLayout(isMine);
            ApplyTheme(ThemeManager.IsDark);

            flpMessageContent.Controls.Clear();
            flpMessageContent.Padding = new Padding(6);

            PictureBox pic = new PictureBox();
            pic.SizeMode = PictureBoxSizeMode.Zoom; // Zoom = fit nguyên ảnh, không crop
            pic.Margin = new Padding(0, 2, 0, 2);
            pic.Image = thumbnail;

            // Ảnh fit vào khung maxW/maxH, giữ đúng tỷ lệ
            pic.Size = GetFitSize(thumbnail, IMG_MAX_W, IMG_MAX_H);

            //form ngoài gắn click mở full
            pic.Cursor = Cursors.Hand;
            pic.TabStop = false;

            flpMessageContent.Controls.Add(pic);

            if (!string.IsNullOrWhiteSpace(caption))
            {
                Label lbl = new Label();
                lbl.Text = caption;
                lbl.AutoSize = true;
                lbl.MaximumSize = new Size(TEXT_MAX_W, 0);
                lbl.Font = new Font("Segoe UI", 9.5F);
                lbl.BackColor = Color.Transparent;
                lbl.Margin = new Padding(0, 4, 0, 0);

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

            FitBubbleHeight();
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

        #region ======= IMAGE HELPER =======

        /// <summary>
        /// Tính size "fit" theo khung maxW x maxH, giữ tỉ lệ ảnh.
        /// - Ảnh to: thu nhỏ cho vừa
        /// - Ảnh nhỏ: giữ nguyên (không upscale)
        /// </summary>
        private static Size GetFitSize(Image img, int maxW, int maxH)
        {
            if (img == null || img.Width <= 0 || img.Height <= 0)
            {
                return new Size(240, 160);
            }

            double scaleW = (double)maxW / img.Width;
            double scaleH = (double)maxH / img.Height;

            // Fit vào khung => lấy scale nhỏ hơn (không crop)
            double scale = Math.Min(scaleW, scaleH);

            // Không upscale (ảnh nhỏ giữ nguyên)
            if (scale > 1.0) scale = 1.0;
            if (scale <= 0) scale = 1.0;

            int w = Math.Max(1, (int)Math.Round(img.Width * scale));
            int h = Math.Max(1, (int)Math.Round(img.Height * scale));
            return new Size(w, h);
        }

        /// <summary>
        /// Khi nội dung (ảnh/text) cao hơn -> bị cắt.
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
            if (!DesignMode) ApplyTheme(ThemeManager.IsDark);
        }

        #endregion
    }
}