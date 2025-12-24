using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using ChatApp.Services.Firebase;
using System.Threading.Tasks;
using ChatApp.Controllers;

namespace ChatApp.Controls
{
    /// <summary>
    /// MessageBubbles: UI hiển thị 1 tin nhắn trong khung chat.
    /// - Hỗ trợ tin nhắn text (kèm emoji dạng :name:)
    /// - Hỗ trợ tin nhắn ảnh (thumbnail + caption)
    /// - Tự canh trái/phải theo isMine
    /// - Tự đổi theme (dark/light)
    /// - Tự tăng Height theo nội dung để tránh bị cắt
    /// </summary>
    public partial class MessageBubbles : UserControl
    {
        #region ====== BIẾN DÙNG CHUNG ======

        private readonly AuthService _authService;

        /// <summary>
        /// Tin nhắn của mình hay của người khác.
        /// </summary>
        private bool IsMine;

        /// <summary>
        /// Giới hạn kích thước ảnh hiển thị để không làm vỡ layout.
        /// </summary>
        private const int IMG_MAX_W = 320;
        private const int IMG_MAX_H = 240;

        /// <summary>
        /// Giới hạn bề ngang chữ để tự xuống dòng.
        /// </summary>
        private const int TEXT_MAX_W = 280;

        #endregion

        #region ====== AVATAR (TẢI + PLACEHOLDER) ======

        private readonly AvatarController _avatarController = new AvatarController();
        /// <summary>
        /// Tải avatar theo senderId (localId) và set lên picAvatar.
        /// AvatarController có cache, nên gọi nhiều lần vẫn ổn.
        /// </summary>
        public async Task LoadAvatarAsync(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;
            if (picAvatar == null || picAvatar.IsDisposed) return;

            // AvatarController sẽ tự:
            // - set ảnh mặc định default_avatar.png
            // - rồi tải avatar thật (nếu có) từ Firebase để thay thế
            await _avatarController.LoadAvatarToPictureBoxAsync(senderId, picAvatar).ConfigureAwait(true);
        }

        

        #endregion

        #region ====== KHỞI TẠO CONTROL ======

        public MessageBubbles()
        {
            InitializeComponent();

            // _authService hiện chưa dùng trong logic bên dưới,
            // giữ lại nếu bạn có kế hoạch mở rộng (status, profile, v.v)
            _authService = new AuthService();
        }

        #endregion

        #region ====== API HIỂN THỊ TIN NHẮN ======

        /// <summary>
        /// Hiển thị tin nhắn text (có thể kèm emoji :name:).
        /// </summary>
        public async Task SetMessage(string senderId, string displayName, string message, string time, bool isMine)
        {
            if (lblDisplayName != null)
            {
                lblDisplayName.Text = displayName;
            }

            if (lblTime != null)
            {
                lblTime.Text = time;
            }

            IsMine = isMine;

            // 1) Canh trái/phải
            ApplyLayout(IsMine);

            // 2) Màu bubble theo theme
            ApplyTheme(ThemeManager.IsDark);

            // 3) Render nội dung
            if (flpMessageContent != null)
            {
                flpMessageContent.Controls.Clear();
                RenderTextWithEmoji(message);
            }

            // 4) Fit height tránh bị cắt
            FitBubbleHeight();

            // 5) Load avatar
            await LoadAvatarAsync(senderId);
        }

        /// <summary>
        /// Hiển thị tin nhắn ảnh (thumbnail + caption).
        /// Lưu ý: Click mở full ảnh do controller bên ngoài gắn sự kiện.
        /// </summary>
        public void SetImageMessage(string displayName, Image thumbnail, string caption, string time, bool isMine)
        {
            if (lblDisplayName != null)
            {
                lblDisplayName.Text = displayName;
            }

            if (lblTime != null)
            {
                lblTime.Text = time;
            }

            IsMine = isMine;

            ApplyLayout(IsMine);
            ApplyTheme(ThemeManager.IsDark);

            if (flpMessageContent == null)
            {
                return;
            }

            flpMessageContent.Controls.Clear();
            flpMessageContent.Padding = new Padding(6);

            PictureBox pic = new PictureBox();
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Margin = new Padding(0, 2, 0, 2);
            pic.Image = thumbnail;
            pic.Size = GetFitSize(thumbnail, IMG_MAX_W, IMG_MAX_H);
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

        #region ====== RENDER TEXT + EMOJI ======

        private void RenderTextWithEmoji(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // Tách emoji dạng :name: ra khỏi text để render PictureBox
            string pattern = @"(:[a-zA-Z0-9_]+:)";
            string[] parts = Regex.Split(message, pattern);

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                bool isEmojiToken = Regex.IsMatch(part, pattern);
                if (isEmojiToken)
                {
                    bool rendered = TryAddEmoji(part);
                    if (!rendered)
                    {
                        AddTextLabel(part);
                    }
                }
                else
                {
                    AddTextLabel(part);
                }
            }
        }

        private bool TryAddEmoji(string token)
        {
            // token ví dụ ":smile:"
            string emojiName = token.Trim(':');
            string path = Path.Combine(Application.StartupPath, "Resources", "Emoji", emojiName + ".png");

            if (!File.Exists(path))
            {
                return false;
            }

            PictureBox pic = new PictureBox();
            pic.Image = Image.FromFile(path);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Size = new Size(24, 24);
            pic.Margin = new Padding(1, 1, 1, 0);

            flpMessageContent.Controls.Add(pic);
            return true;
        }

        private void AddTextLabel(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Label lbl = new Label();
            lbl.Text = text;
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", 10.5F);
            lbl.BackColor = Color.Transparent;
            lbl.Margin = new Padding(0, 3, 0, 0);

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

        #endregion

        #region ====== HỖ TRỢ HIỂN THỊ ẢNH ======

        /// <summary>
        /// Tính size "fit" theo khung maxW x maxH, giữ tỉ lệ ảnh.
        /// - Ảnh lớn: thu nhỏ cho vừa
        /// - Ảnh nhỏ: giữ nguyên (không phóng to)
        /// </summary>
        private static Size GetFitSize(Image img, int maxW, int maxH)
        {
            if (img == null || img.Width <= 0 || img.Height <= 0)
            {
                return new Size(240, 160);
            }

            double scaleW = (double)maxW / (double)img.Width;
            double scaleH = (double)maxH / (double)img.Height;

            double scale = Math.Min(scaleW, scaleH);

            if (scale > 1.0)
            {
                scale = 1.0;
            }

            if (scale <= 0.0)
            {
                scale = 1.0;
            }

            int w = (int)Math.Round(img.Width * scale);
            int h = (int)Math.Round(img.Height * scale);

            if (w < 1) w = 1;
            if (h < 1) h = 1;

            return new Size(w, h);
        }

        /// <summary>
        /// Sau khi render xong nội dung (text/ảnh), bubble có thể cao hơn.
        /// => Lấy PreferredSize của pnlBackGround rồi set Height để khỏi bị cắt.
        /// </summary>
        private void FitBubbleHeight()
        {
            try
            {
                if (flpMessageContent != null) flpMessageContent.PerformLayout();
                if (pnlBubble != null) pnlBubble.PerformLayout();
                if (flpBubble != null) flpBubble.PerformLayout();
                if (pnlBackGround != null) pnlBackGround.PerformLayout();

                int targetH = pnlBackGround != null ? pnlBackGround.PreferredSize.Height : 0;
                if (targetH <= 0 && pnlBackGround != null)
                {
                    targetH = pnlBackGround.Height;
                }

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

        #region ====== THEME & CANH LAYOUT TRÁI/PHẢI ======

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

        #region ====== VÒNG ĐỜI CONTROL ======

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
