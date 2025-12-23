using ChatApp.Forms;
using ChatApp.Models.Messages;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller mở ImageViewer từ tin nhắn "image".
    /// </summary>
    public class ImageViewerController
    {
        public async Task ShowFromMessageAsync(ChatMessage msg, IWin32Window owner)
        {
            // Cho UI thread nhường 1 tick để click "ăn" trước
            await Task.Yield();

            if (msg == null) return;

            if (!string.Equals(msg.MessageType, "image", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(msg.ImageBase64))
            {
                MessageBox.Show(owner, "Tin nhắn ảnh bị thiếu dữ liệu (ImageBase64).", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(msg.ImageBase64);
            }
            catch
            {
                MessageBox.Show(owner, "Ảnh bị lỗi/không đọc được (base64 sai).", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = string.IsNullOrWhiteSpace(msg.FileName) ? "image" : msg.FileName;

            using (ImageViewer viewer = new ImageViewer(bytes, fileName, msg.ImageMimeType))
            {
                viewer.ShowDialog(owner);
            }
        }
    }
}
