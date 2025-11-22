using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class EmojiPickerForm : Form
    {
        /// <summary>
        /// Key emoji được chọn (tên file, ví dụ: "smile.png").
        /// NhanTin sẽ dùng key này để gửi lên Firebase.
        /// </summary>
        public string SelectedEmojiKey { get; private set; }

        private FlowLayoutPanel _panel;

        public EmojiPickerForm()
        {
            InitializeComponent();
            InitializeEmojiUi();
            LoadEmojis();
        }

        /// <summary>
        /// Tạo layout cơ bản (FlowLayoutPanel chứa các emoji).
        /// </summary>
        private void InitializeEmojiUi()
        {
            this.Text = "Chọn Emoji";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 360;
            this.Height = 320;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _panel = new FlowLayoutPanel();
            _panel.Dock = DockStyle.Fill;
            _panel.AutoScroll = true;
            _panel.WrapContents = true;
            _panel.Padding = new Padding(8);
            _panel.FlowDirection = FlowDirection.LeftToRight;

            this.Controls.Add(_panel);
        }

        /// <summary>
        /// Load tất cả file PNG trong thư mục resources/Emoji
        /// và hiển thị bằng PictureBox 72x72.
        /// </summary>
        private void LoadEmojis()
        {
            string root = Application.StartupPath;
            string path = Path.Combine(root, "Resources", "Emoji");

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Không tìm thấy thư mục Emoji: " + path,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(path, "*.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc thư mục Emoji: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (string file in files)
            {
                string emojiKey = Path.GetFileName(file); // dùng tên file làm key

                var pic = new PictureBox();
                pic.Width = 20;
                pic.Height = 20;
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Margin = new Padding(4);
                pic.Cursor = Cursors.Hand;

                try
                {
                    // load ảnh
                    pic.Image = Image.FromFile(file);
                }
                catch
                {
                    continue; // bỏ qua file lỗi
                }

                pic.Tag = emojiKey;
                pic.Click += Pic_Click;

                _panel.Controls.Add(pic);
            }
        }

        /// <summary>
        /// Khi chọn 1 emoji => set SelectedEmojiKey và đóng form.
        /// </summary>
        private void Pic_Click(object sender, EventArgs e)
        {
            var pic = sender as PictureBox;
            if (pic == null)
                return;

            string key = pic.Tag as string;
            if (string.IsNullOrEmpty(key))
                return;

            SelectedEmojiKey = key;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
