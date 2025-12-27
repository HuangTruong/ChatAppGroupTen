using ChatApp.Helpers;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
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

        // ===== LIMIT MESSAGE =====
        private const int MESSAGE_MAX_VISIBLE_CHARS = 200; // Bạn muốn ngắn hơn thì giảm số này
        private const int TEXT_MAX_W = 200;               // Giới hạn bề ngang để tự xuống dòng

        private string _fullMessage = string.Empty;
        private bool _isExpanded = false;
        private bool _canToggle = false;

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

            // 3. Lưu message gốc + render có giới hạn
            _fullMessage = message ?? string.Empty;
            _isExpanded = false;

            RenderMessageWithLimit();

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

        #region ======= LIMIT MESSAGE =======

        /// <summary>
        /// Render message với giới hạn độ dài + thêm "Xem thêm/Thu gọn" nếu cần.
        /// </summary>
        private void RenderMessageWithLimit()
        {
            flpMessageContent.Controls.Clear();

            string displayMessage;
            bool truncated;

            if (_isExpanded)
            {
                displayMessage = _fullMessage;
                truncated = false;
                _canToggle = IsMessageExceedLimit(_fullMessage);
            }
            else
            {
                displayMessage = TruncateMessagePreserveEmoji(_fullMessage, MESSAGE_MAX_VISIBLE_CHARS, out truncated);
                _canToggle = truncated;
            }

            RenderContent(displayMessage);

            if (_canToggle)
            {
                flpMessageContent.Controls.Add(CreateToggleLinkLabel());
            }

            FitBubbleHeight();
        }

        /// <summary>
        /// Kiểm tra message có vượt giới hạn hay không (tính emoji token như 1 ký tự).
        /// </summary>
        private bool IsMessageExceedLimit(string message)
        {
            bool truncated;
            TruncateMessagePreserveEmoji(message, MESSAGE_MAX_VISIBLE_CHARS, out truncated);
            return truncated;
        }

        /// <summary>
        /// Cắt message theo "độ dài hiển thị", đảm bảo không phá token emoji (:name:).
        /// Emoji token được tính như 1 ký tự hiển thị.
        /// </summary>
        private string TruncateMessagePreserveEmoji(string message, int maxVisibleChars, out bool truncated)
        {
            truncated = false;
            if (string.IsNullOrEmpty(message)) return string.Empty;
            if (maxVisibleChars <= 0) { truncated = true; return "…"; }

            string pattern = @"(:[a-zA-Z0-9_]+:)";
            string[] parts = Regex.Split(message, pattern);

            StringBuilder sb = new StringBuilder();
            int used = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part)) continue;

                bool isEmojiToken = Regex.IsMatch(part, pattern);

                if (isEmojiToken)
                {
                    // Emoji token tính như 1 ký tự hiển thị
                    if (used + 1 > maxVisibleChars)
                    {
                        truncated = true;
                        break;
                    }

                    sb.Append(part);
                    used += 1;
                }
                else
                {
                    int remaining = maxVisibleChars - used;
                    if (remaining <= 0)
                    {
                        truncated = true;
                        break;
                    }

                    if (part.Length <= remaining)
                    {
                        sb.Append(part);
                        used += part.Length;
                    }
                    else
                    {
                        // Cắt text và ưu tiên cắt theo khoảng trắng (đỡ cụt từ)
                        string cut = part.Substring(0, remaining);
                        int lastSpace = cut.LastIndexOf(' ');
                        if (lastSpace >= 20) // tránh trường hợp cắt quá ngắn
                        {
                            cut = cut.Substring(0, lastSpace);
                        }

                        sb.Append(cut);
                        used += cut.Length;

                        truncated = true;
                        break;
                    }
                }
            }

            if (truncated)
            {
                sb.Append("…");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Tạo link "Xem thêm/Thu gọn" để toggle.
        /// </summary>
        private LinkLabel CreateToggleLinkLabel()
        {
            LinkLabel lnk = new LinkLabel();
            lnk.AutoSize = true;
            lnk.Text = _isExpanded ? "Thu gọn" : "Xem thêm";
            lnk.Margin = new Padding(4, 2, 4, 0);
            lnk.BackColor = Color.Transparent;
            lnk.LinkBehavior = LinkBehavior.HoverUnderline;

            if (ThemeManager.IsDark)
            {
                lnk.LinkColor = ColorTranslator.FromHtml("#60A5FA");
                lnk.ActiveLinkColor = ColorTranslator.FromHtml("#93C5FD");
                lnk.VisitedLinkColor = ColorTranslator.FromHtml("#60A5FA");
            }
            else
            {
                lnk.LinkColor = ColorTranslator.FromHtml("#0284C7");
                lnk.ActiveLinkColor = ColorTranslator.FromHtml("#0369A1");
                lnk.VisitedLinkColor = ColorTranslator.FromHtml("#0284C7");
            }

            lnk.Click += delegate
            {
                _isExpanded = !_isExpanded;
                RenderMessageWithLimit();
            };

            return lnk;
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
                BackColor = Color.Transparent,
                MaximumSize = new Size(TEXT_MAX_W, 0) // cho phép tự xuống dòng, không kéo quá dài ngang
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
                // 1) Ép layout tính lại size
                flpMessageContent.PerformLayout();
                pnlBubble.PerformLayout();
                flpBubble.PerformLayout();
                pnlBackGround.PerformLayout();
                this.PerformLayout();

                // 2) Đảm bảo label tự tính chiều cao đúng
                // (nếu bạn dùng AutoSize=true thì không cần, nhưng để chắc chắn)
                lblDisplayName.AutoSize = true;
                lblTime.AutoSize = true;

                // 3) Lấy "chiều cao cần thiết" từ container bao hết mọi thứ (flpBubble)
                int needH = flpBubble.PreferredSize.Height;

                if (needH <= 0) needH = flpBubble.Height;

                // 4) Cộng thêm padding/margin để không bị sát mép
                // - Margin của flpBubble (so với parent)
                needH += flpBubble.Margin.Top + flpBubble.Margin.Bottom;

                // - Padding của UserControl (nếu có)
                needH += this.Padding.Top + this.Padding.Bottom;

                // - Trường hợp flpBubble nằm trong pnlBackGround có Padding
                needH += pnlBackGround.Padding.Top + pnlBackGround.Padding.Bottom;

                // 5) Đảm bảo tối thiểu vẫn vừa avatar (nếu avatar cao hơn nội dung)
                int avatarH = 0;
                try
                {
                    avatarH = pnlAvatar.PreferredSize.Height;
                    if (avatarH <= 0) avatarH = pnlAvatar.Height;
                    avatarH += pnlAvatar.Margin.Top + pnlAvatar.Margin.Bottom;
                }
                catch { }

                if (avatarH > needH) needH = avatarH;

                // 6) Set Height (chỉ tăng để khỏi giật layout)
                if (needH > this.Height)
                {
                    this.Height = needH;
                }
                else
                {
                    // Nếu bạn muốn shrink lại khi "Thu gọn" thì mở dòng này:
                    this.Height = needH;
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
            // ===== Base =====
            pnlBackGround.BackColor = Color.Transparent;

            pnlBubble.BorderRadius = 10;
            pnlBubble.BorderThickness = 1;

            pnlBubble.ShadowDecoration.Enabled = true;
            pnlBubble.ShadowDecoration.Depth = 10;
            pnlBubble.ShadowDecoration.BorderRadius = pnlBubble.BorderRadius;

            picAvatar.ShadowDecoration.Enabled = true;
            picAvatar.ShadowDecoration.Depth = 5;

            if (!isDark)
            {
                // ===== DAY TEXT =====
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#0B1B2B");
                lblTime.ForeColor = ColorTranslator.FromHtml("#2F3E4D");

                if (IsMine)
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#D9FFFE");
                    pnlBubble.BorderColor = ColorTranslator.FromHtml("#7BC7FF");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#00C2FF");

                    picAvatar.ShadowDecoration.Color = ColorTranslator.FromHtml("#00C2FF");
                }
                else
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#FFFFFF");
                    pnlBubble.BorderColor = ColorTranslator.FromHtml("#9AD6FF");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#00C2FF");

                    picAvatar.ShadowDecoration.Color = ColorTranslator.FromHtml("#00C2FF");
                }
            }
            else
            {
                // ===== NIGHT TEXT  =====
                lblDisplayName.ForeColor = ColorTranslator.FromHtml("#FFD36E");
                lblTime.ForeColor = ColorTranslator.FromHtml("#9AD0FF");

                if (IsMine)
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#0A1E3F");
                    pnlBubble.BorderColor = ColorTranslator.FromHtml("#2EF2FF");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#2EF2FF");

                    picAvatar.ShadowDecoration.Color = ColorTranslator.FromHtml("#2EF2FF");
                }
                else
                {
                    pnlBubble.FillColor = ColorTranslator.FromHtml("#050814");
                    pnlBubble.BorderColor = ColorTranslator.FromHtml("#1A2B4A");
                    pnlBubble.ShadowDecoration.Color = ColorTranslator.FromHtml("#2EF2FF");

                    picAvatar.ShadowDecoration.Color = ColorTranslator.FromHtml("#2EF2FF");
                }
            }
        }

        private void ApplyLayout(bool isMine)
        {
            if (IsMine)
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
