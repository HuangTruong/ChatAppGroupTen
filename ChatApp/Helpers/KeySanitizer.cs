using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ChatApp.Helpers
{
    // Làm sạch (sanitize) chuỗi trước khi dùng làm khóa (key) trong Firebase Realtime Database.
    public static class KeySanitizer
    {
        // Làm sạch chuỗi để dùng làm key trên Firebase
        public static string SafeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string key = raw.Trim();
            char[] invalid = { '.', '#', '$', '[', ']', '/', ' ' };

            foreach (char c in invalid)
            {
                key = key.Replace(c, '_');
            }

            return key;
        }
    }
}
