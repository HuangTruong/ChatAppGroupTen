using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class FormEmoji : Form
    {
        // 1. Tạo một Action để gửi mã emoji về Form nhắn tin
        public Action<string> OnEmojiSelected;

        public FormEmoji()
        {
            InitializeComponent();

            // Tự động đóng form khi người dùng nhấn chuột ra ngoài vùng chat
            this.Deactivate += (s, e) => this.Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadEmojiList();
        }

        private void LoadEmojiList()
        {
            // Xóa các control cũ nếu có
            flpEmojis.Controls.Clear();

            // Đường dẫn tới thư mục chứa ảnh emoji
            string emojiPath = Path.Combine(Application.StartupPath, "Resources", "Emoji");

            if (Directory.Exists(emojiPath))
            {
                string[] files = Directory.GetFiles(emojiPath, "*.png");
                foreach (string file in files)
                {
                    Guna.UI2.WinForms.Guna2PictureBox pic = new Guna.UI2.WinForms.Guna2PictureBox();
                    pic.Image = Image.FromFile(file);
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                    pic.Size = new Size(30, 30);
                    pic.Cursor = Cursors.Hand;
                    pic.Margin = new Padding(5);
                    pic.BackColor = Color.Transparent;

                    // Lấy tên file (ví dụ "smile") làm mã code
                    string emojiCode = Path.GetFileNameWithoutExtension(file);

                    // Khi click vào icon
                    pic.Click += (s, e) =>
                    {
                        // Gửi mã code về Form chính thông qua Action
                        OnEmojiSelected?.Invoke(emojiCode);
                        this.Close(); // Đóng bảng chọn sau khi chọn xong
                    };

                    flpEmojis.Controls.Add(pic);
                }
            }
        }
    }
}