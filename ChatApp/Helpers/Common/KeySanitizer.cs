using System;

namespace ChatApp.Helpers
{
    /// <summary>
    /// Helper dùng để "làm sạch" (sanitize) chuỗi trước khi sử dụng làm key
    /// trong Firebase Realtime Database:
    /// Firebase không cho phép các ký tự đặc biệt như: <c>., #, $, [, ], /, (space)</c>.
    /// Hàm này thay thế chúng bằng dấu gạch dưới để chuỗi an toàn hơn.
    /// </summary>
    public static class KeySanitizer
    {
        #region ======== Làm sạch chuỗi để dùng làm key Firebase ========

        /// <summary>
        /// Làm sạch chuỗi để dùng làm khóa trên Firebase:
        /// - Trim hai đầu.
        /// - Thay thế toàn bộ ký tự cấm bằng dấu <c>_</c>.
        /// - Trả về chuỗi rỗng nếu đầu vào null hoặc whitespace.
        /// </summary>
        /// <param name="raw">Chuỗi raw từ người dùng nhập hoặc từ tên tài khoản.</param>
        /// <returns>
        /// Chuỗi đã chuẩn hóa, an toàn để dùng làm key trên Firebase.
        /// </returns>
        public static string SafeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string key = raw.Trim();

            // Các ký tự cấm trong Firebase Realtime Database
            char[] invalid = { '.', '#', '$', '[', ']', '/', ' ' };

            foreach (char c in invalid)
                key = key.Replace(c, '_');

            return key;
        }

        #endregion
    }
}
