using System;

namespace ChatApp.Helpers
{
    /// <summary>
    /// Cung cấp hàm "làm sạch" key để dùng làm đường dẫn / node trên Firebase.
    /// Firebase Realtime Database không cho phép một số ký tự đặc biệt trong key.
    /// </summary>
    public static class KeySanitizer
    {
        #region ====== SAFE KEY ======

        /// <summary>
        /// Chuyển một chuỗi bất kỳ về dạng "an toàn" để làm key trên Firebase.
        /// Thay thế các ký tự bị cấm (. $ # [ ] / và khoảng trắng) bằng dấu gạch dưới "_".
        /// </summary>
        /// <param name="key">Chuỗi key gốc.</param>
        /// <returns>
        /// Chuỗi key đã được làm sạch; trả về chuỗi rỗng nếu input null/whitespace.
        /// </returns>
        public static string SafeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            key = key.Trim();

            // Thay các ký tự Firebase không cho phép bằng "_"
            key = key.Replace(".", "_")
                     .Replace("$", "_")
                     .Replace("#", "_")
                     .Replace("[", "_")
                     .Replace("]", "_")
                     .Replace("/", "_")
                     .Replace(" ", "_");

            return key;
        }

        #endregion
    }
}
