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
        private static readonly Regex _bad = new Regex(@"[.#$\[\]/]", RegexOptions.Compiled);
        public static string SafeKey(string s) => string.IsNullOrEmpty(s) ? s : _bad.Replace(s, "_");
    }
}
