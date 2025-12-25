using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    /// <summary>
    /// Form xem ảnh full (zoom) + nút Download ở dưới.
    /// </summary>
    public class ImageViewerForm : Form
    {
        private readonly byte[] _bytes;
        private readonly string _fileName;
        private readonly string _mimeType;

        private PictureBox _pic;
        private Button _btnDownload;

        public ImageViewerForm(byte[] bytes, string fileName, string mimeType = null)
        {
            _bytes = bytes ?? Array.Empty<byte>();
            _fileName = string.IsNullOrWhiteSpace(fileName) ? "image" : fileName;
            _mimeType = mimeType ?? "image/*";

            BuildUi();
            LoadImage();
        }

        private void BuildUi()
        {
            Text = _fileName;
            StartPosition = FormStartPosition.CenterParent;
            Width = 980;
            Height = 720;
            BackColor = Color.Black;
            KeyPreview = true;

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };

            _pic = new PictureBox();
            _pic.Dock = DockStyle.Fill;
            _pic.SizeMode = PictureBoxSizeMode.Zoom;
            _pic.BackColor = Color.Black;
            _pic.Click += (s, e) => { /* Click ảnh không làm gì; user có thể bấm ESC để đóng */ };

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 60;
            bottom.BackColor = Color.FromArgb(22, 22, 22);

            _btnDownload = new Button();
            _btnDownload.Text = "Download";
            _btnDownload.Width = 140;
            _btnDownload.Height = 38;
            _btnDownload.FlatStyle = FlatStyle.Flat;
            _btnDownload.FlatAppearance.BorderSize = 1;
            _btnDownload.ForeColor = Color.White;
            _btnDownload.BackColor = Color.FromArgb(35, 35, 35);
            _btnDownload.Anchor = AnchorStyles.None;
            _btnDownload.Click += BtnDownload_Click;

            bottom.Controls.Add(_btnDownload);

            Controls.Add(_pic);
            Controls.Add(bottom);

            bottom.Resize += (s, e) =>
            {
                _btnDownload.Left = (bottom.ClientSize.Width - _btnDownload.Width) / 2;
                _btnDownload.Top = (bottom.ClientSize.Height - _btnDownload.Height) / 2;
            };
        }

        private void LoadImage()
        {
            if (_bytes == null || _bytes.Length == 0) return;

            try
            {
                using (MemoryStream ms = new MemoryStream(_bytes))
                using (Image tmp = Image.FromStream(ms))
                {
                    // Clone để tránh lệ thuộc stream đã dispose
                    _pic.Image = new Bitmap(tmp);
                }
            }
            catch
            {
                // Nếu ảnh lỗi, giữ trống
            }
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    string downloads = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Downloads");

                    if (Directory.Exists(downloads))
                    {
                        sfd.InitialDirectory = downloads;
                    }

                    sfd.FileName = _fileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    File.WriteAllBytes(sfd.FileName, _bytes);
                    MessageBox.Show(this, "Đã tải về:\n" + sfd.FileName, "Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Download lỗi: " + ex.Message, "Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _pic?.Image?.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
