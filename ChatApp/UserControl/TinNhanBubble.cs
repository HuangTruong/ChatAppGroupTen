using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Net.Http;

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

        // THÊM: PictureBox hiển thị emoji
        private PictureBox picEmoji = new PictureBox();

        // ==== THÊM: Gửi file ====
        public bool LaFile { get; set; } = false; 
        public string TenFile { get; set; }
        public long KichThuoc { get; set; }
        public string FileUrl { get; set; }

        // Bubble file
        private Panel pnlFile;
        private Label lblFileName;
        private Label lblFileSize;

        public TinNhanBubble()
        {
            InitializeComponent();

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

            picEmoji.Width = 20;
            picEmoji.Height = 20;
            picEmoji.SizeMode = PictureBoxSizeMode.Zoom;
            picEmoji.Visible = false;

            // ==== BUBBLE FILE ====
            pnlFile = new Panel
            {
                BackColor = Color.FromArgb(230, 230, 230),
                Visible = false,
                Padding = new Padding(6),
                Cursor = Cursors.Hand
            };
            // Cho panel file tự co giãn theo nội dung
            pnlFile.AutoSize = true;
            pnlFile.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            lblFileName = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(220, 0),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(6, 4)
            };

            lblFileSize = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(220, 0),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8F),
                Location = new Point(6, 24)
            };

            pnlFile.Controls.Add(lblFileName);
            pnlFile.Controls.Add(lblFileSize);

            // Click vào panel hoặc label trong panel đều tải file
            pnlFile.Click += FileClick;
            lblFileName.Click += FileClick;
            lblFileSize.Click += FileClick;

            // Thêm control vào bubble
            this.Controls.Add(lblTen);
            this.Controls.Add(lblNoiDung);
            this.Controls.Add(picEmoji);
            this.Controls.Add(lblGio);
            this.Controls.Add(pnlFile);

            // Khi size thay đổi thì layout lại
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

            // ================== NỘI DUNG / EMOJI / FILE ==================

            // Reset style & ẩn hết trước
            lblNoiDung.ForeColor = Color.Black;
            lblNoiDung.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblNoiDung.Cursor = Cursors.IBeam;

            lblNoiDung.Visible = false;
            picEmoji.Visible = false;
            pnlFile.Visible = false;

            if (LaFile && !string.IsNullOrEmpty(TenFile))
            {
                // ---- CASE FILE ----
                pnlFile.Visible = true;

                // Tên file
                lblFileName.Text = TenFile;

                // Kích thước
                string sizeText = "";
                if (KichThuoc > 0)
                {
                    double mb = KichThuoc / (1024.0 * 1024.0);
                    sizeText = mb >= 1
                        ? $"{mb:0.##} MB"
                        : $"{KichThuoc / 1024.0:0.##} KB";
                }
                lblFileSize.Text = string.IsNullOrEmpty(sizeText)
                    ? "Nhấn để tải xuống"
                    : sizeText;

                // Cho cảm giác là link
                lblFileName.ForeColor = Color.Blue;
                lblFileName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            }
            else if (LaEmoji && !string.IsNullOrEmpty(EmojiKey))
            {
                // ---- CASE EMOJI ----
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
                // ---- CASE TEXT THƯỜNG ----
                lblNoiDung.Visible = true;
                lblNoiDung.Text = NoiDung;
            }

            // Thời gian
            var local = ThoiGianUtc.ToLocalTime();
            lblGio.Text = local.ToString("HH:mm dd/MM/yyyy");

            // Bo góc (giữ logic cũ)
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
            int x = 10;
            int y = 8;

            // Tên
            if (lblTen.Visible)
            {
                lblTen.Location = new Point(x, y);
                y = lblTen.Bottom + 4;
            }

            // Nội dung chính: FILE / EMOJI / TEXT
            if (pnlFile != null && pnlFile.Visible)
            {
                // bubble file
                pnlFile.Location = new Point(x, y);
                y = pnlFile.Bottom + 6;
            }
            else if (picEmoji.Visible)
            {
                // emoji
                picEmoji.Location = new Point(x, y);
                y = picEmoji.Bottom + 6;
            }
            else if (lblNoiDung.Visible)
            {
                // text thường
                lblNoiDung.Location = new Point(x, y);
                y = lblNoiDung.Bottom + 6;
            }

            // Giờ
            lblGio.Location = new Point(x, y);

            // chiều cao control
            this.Height = lblGio.Bottom + 10;

            // Tính chiều rộng theo cái đang hiển thị
            int contentRight = 0;
            if (pnlFile != null && pnlFile.Visible)
                contentRight = pnlFile.Right;
            else if (picEmoji.Visible)
                contentRight = picEmoji.Right;
            else if (lblNoiDung.Visible)
                contentRight = lblNoiDung.Right;

            this.Width = Math.Max(contentRight + 20, lblGio.Right + 20);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "TinNhanBubble";
            this.Load += new System.EventHandler(this.TinNhanBubble_Load);
            this.ResumeLayout(false);
        }

        // Xử lý khi click vào bubble file
        private async void FileClick(object sender, EventArgs e)
        {
            // Chỉ xử lý khi là tin nhắn file
            if (!LaFile || string.IsNullOrEmpty(FileUrl))
                return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = string.IsNullOrEmpty(TenFile) ? "file" : TenFile;
                sfd.Filter = "All files (*.*)|*.*";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // Link http/https → tải bằng HttpClient
                    if (Uri.TryCreate(FileUrl, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        using (var client = new HttpClient())
                        using (var resp = await client.GetAsync(FileUrl))
                        {
                            resp.EnsureSuccessStatusCode();

                            using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                            {
                                await resp.Content.CopyToAsync(fs);
                            }
                        }
                    }
                    else
                    {
                        File.Copy(FileUrl, sfd.FileName, overwrite: true);
                    }

                    MessageBox.Show("Tải file xong rồi đó!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Tải file thất bại: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TinNhanBubble_Load(object sender, EventArgs e)
        {
        }
    }
}
