using System;
using System.Drawing;
using System.IO;

namespace ChatApp.Helpers
{
    public static class ImageBase64
    {
        /// <summary>
        /// Chuyển Image sang Base64 (lưu Firebase)
        /// </summary>
        public static string ImageToBase64(Image image)
        {
            if (image == null) return null;

            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Chuyển base64 sang Image để hiển thị
        /// </summary>
        public static Image Base64ToImage(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64) ||
                base64.Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(bytes))
                {
                    using (var img = Image.FromStream(ms))
                    {
                        return (Image)img.Clone(); // tránh ảnh đen
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
