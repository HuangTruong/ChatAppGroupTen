using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class ImageViewer : Form
    {
        private readonly byte[] _bytes;
        private readonly string _fileName;
        private readonly string _mimeType;

        public ImageViewer(byte[] bytes, string fileName, string mimeType = null)
        {
            _bytes = bytes ?? Array.Empty<byte>();
            _fileName = string.IsNullOrWhiteSpace(fileName) ? "image" : fileName;
            _mimeType = mimeType ?? "image/*";

            InitializeComponent();
            LoadImage();
        }

        private void ImageViewerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void LoadImage()
        {
            if (_bytes == null || _bytes.Length == 0) return;

            try
            {
                using (MemoryStream ms = new MemoryStream(_bytes))
                using (Image tmp = Image.FromStream(ms))
                {
                    pictureImage.Image = new Bitmap(tmp);
                }
            }
            catch
            {
                // ảnh lỗi → bỏ trống
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    string downloads = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Downloads");

                    if (Directory.Exists(downloads))
                        sfd.InitialDirectory = downloads;

                    sfd.FileName = _fileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog(this) != DialogResult.OK)
                        return;

                    File.WriteAllBytes(sfd.FileName, _bytes);

                    MessageBox.Show(
                        this,
                        "Đã tải về:\n" + sfd.FileName,
                        "Download",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Download lỗi: " + ex.Message,
                    "Download",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
