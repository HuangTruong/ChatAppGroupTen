using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    public class TinNhanBubble : UserControl
    {
        public bool LaCuaToi { get; set; }
        public bool LaNhom { get; set; }
        public string TenNguoiGui { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public DateTime ThoiGianUtc { get; set; } = DateTime.UtcNow;

        // ==== THÊM: hỗ trợ emoji ====
        public bool LaEmoji { get; set; } = false;
        public string EmojiKey { get; set; } = null; // ví dụ: "smile.png"

        private Label lblTen = new Label();
        private Label lblNoiDung = new Label();
        private Label lblGio = new Label();

        // THÊM: PictureBox hiển thị emoji 72x72
        private PictureBox picEmoji = new PictureBox();

        public TinNhanBubble()
        {
            this.Margin = new Padding(6, 4, 6, 4);
            this.Padding = new Padding(10);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = Color.Transparent;

            lblTen.AutoSize = true;
            lblTen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            lblNoiDung.MaximumSize = new Size(420, 0);
            lblNoiDung.AutoSize = true;
            lblNoiDung.Font = new Font("Segoe UI", 10F);
            lblNoiDung.ForeColor = Color.Black;

            lblGio.AutoSize = true;
            lblGio.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblGio.ForeColor = Color.DimGray;

            // Cấu hình emoji 72x72
            picEmoji.Width = 20;
            picEmoji.Height = 20;
            picEmoji.SizeMode = PictureBoxSizeMode.Zoom;
            picEmoji.Visible = false;

            this.Controls.Add(lblTen);
            this.Controls.Add(lblNoiDung);
            this.Controls.Add(picEmoji);
            this.Controls.Add(lblGio);

            this.Resize += (s, e) => CanLayout();
        }

        public void Render()
        {
            // Màu nền & canh trái/phải
            if (LaCuaToi)
            {
                this.BackColor = Color.FromArgb(219, 238, 255);
                this.Anchor = AnchorStyles.Right;
            }
            else
            {
                this.BackColor = Color.FromArgb(240, 240, 240);
                this.Anchor = AnchorStyles.Left;
            }

            // Tên (chỉ hiện khi chat nhóm)
            lblTen.Visible = LaNhom && !string.IsNullOrEmpty(TenNguoiGui);
            lblTen.Text = TenNguoiGui;

            // ====== NỘI DUNG HOẶC EMOJI ======
            if (LaEmoji && !string.IsNullOrEmpty(EmojiKey))
            {
                // Ẩn text, hiện emoji
                lblNoiDung.Visible = false;
                picEmoji.Visible = true;

                string path = Path.Combine(
                    Application.StartupPath,
                    "resources",
                    "Emoji",
                    EmojiKey
                );

                if (File.Exists(path))
                {
                    try
                    {
                        // Nếu muốn tránh lock file, có thể clone image
                        using (var img = Image.FromFile(path))
                        {
                            picEmoji.Image = new Bitmap(img);
                        }
                    }
                    catch
                    {
                        picEmoji.Image = null;
                        picEmoji.Visible = false;
                        lblNoiDung.Visible = true;
                        lblNoiDung.Text = "[emoji lỗi]";
                    }
                }
                else
                {
                    picEmoji.Image = null;
                    picEmoji.Visible = false;
                    lblNoiDung.Visible = true;
                    lblNoiDung.Text = "[emoji lỗi]";
                }
            }
            else
            {
                // Tin nhắn text bình thường
                picEmoji.Visible = false;
                lblNoiDung.Visible = true;
                lblNoiDung.Text = NoiDung;
            }

            // Thời gian
            var local = ThoiGianUtc.ToLocalTime();
            lblGio.Text = local.ToString("HH:mm dd/MM/yyyy");

            // Bo góc
            this.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
                using (var brush = new SolidBrush(this.BackColor))
                {
                    var rect = this.ClientRectangle;
                    rect.Inflate(-1, -1);
                    int r = 12;
                    using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        gp.AddArc(rect.X, rect.Y, r, r, 180, 90);
                        gp.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                        gp.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                        gp.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                        gp.CloseAllFigures();
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, gp);
                        e.Graphics.DrawPath(pen, gp);
                    }
                }
            };

            CanLayout();
        }

        private void CanLayout()
        {
            int x = 10, y = 8;

            if (lblTen.Visible)
            {
                lblTen.Location = new Point(x, y);
                y = lblTen.Bottom + 4;
            }

            // Nếu là emoji → đặt picEmoji, ngược lại đặt lblNoiDung
            if (picEmoji.Visible)
            {
                picEmoji.Location = new Point(x, y);
                y = picEmoji.Bottom + 6;
            }
            else if (lblNoiDung.Visible)
            {
                lblNoiDung.Location = new Point(x, y);
                y = lblNoiDung.Bottom + 6;
            }

            lblGio.Location = new Point(x, y);

            this.Height = lblGio.Bottom + 10;

            int contentRight = 0;
            if (picEmoji.Visible)
                contentRight = picEmoji.Right;
            else if (lblNoiDung.Visible)
                contentRight = lblNoiDung.Right;

            this.Width = Math.Max(contentRight + 20, lblGio.Right + 20);
        }
    }
}
