using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace ChatApp.Services.Attachments
{
    /// <summary>
    /// Phân loại file đầu vào (ảnh hay file thường) dựa trên:
    /// - Extension (nhanh)
    /// - Thử đọc bằng System.Drawing.Image (fallback)
    /// </summary>
    public static class AttachmentClassifier
    {
        private static readonly Dictionary<string, string> _mimeByExt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg",  "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png",  "image/png"  },
            { ".gif",  "image/gif"  },
            { ".bmp",  "image/bmp"  },
            { ".webp", "image/webp" },
            { ".tif",  "image/tiff" },
            { ".tiff", "image/tiff" }
        };

        public static bool IsImageFile(string filePath, out string mimeType)
        {
            mimeType = null;

            if (string.IsNullOrWhiteSpace(filePath)) return false;

            string ext = Path.GetExtension(filePath) ?? string.Empty;

            // 1) Check nhanh theo extension
            if (_mimeByExt.TryGetValue(ext, out string m))
            {
                mimeType = m;
                return true;
            }

            // 2) Fallback: thử mở như ảnh (tránh trường hợp đổi extension)
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image img = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: true))
                {
                    // Nếu mở được thì coi như là ảnh
                    mimeType = "image/*";
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetMimeTypeByExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return "application/octet-stream";
            string ext = Path.GetExtension(filePath) ?? string.Empty;
            if (_mimeByExt.TryGetValue(ext, out string m)) return m;
            return "application/octet-stream";
        }
    }
}
