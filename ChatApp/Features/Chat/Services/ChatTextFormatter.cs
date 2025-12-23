using ChatApp.Models.Users;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Gom các hàm format/normalize text để View không phải "làm việc".
    /// </summary>
    public static class ChatTextFormatter
    {
        #region ====== TIMESTAMP ======

        /// <summary>
        /// Format unix milliseconds -> "dd/MM/yyyy HH:mm".
        /// </summary>
        public static string FormatTimestamp(long timestamp)
        {
            if (timestamp <= 0) return string.Empty;

            try
            {
                DateTime dt = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
                return dt.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region ====== BYTES ======

        /// <summary>
        /// Format bytes thành "KB/MB/GB".
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            double b = bytes;
            string[] u = { "B", "KB", "MB", "GB" };
            int i = 0;

            while (b >= 1024 && i < u.Length - 1)
            {
                b /= 1024;
                i++;
            }

            return string.Format("{0:0.##} {1}", b, u[i]);
        }

        #endregion

        #region ====== FULL NAME ======

        /// <summary>
        /// Lấy tên hiển thị đẹp: ưu tiên FullName, sau đó DisplayName, rồi email trước @.
        /// Đồng thời chuẩn hoá khoảng trắng + TitleCase.
        /// </summary>
        public static string FormatUserFullName(User user)
        {
            if (user == null) return "Người dùng";

            string ten = user.FullName;
            if (string.IsNullOrWhiteSpace(ten)) ten = user.DisplayName;

            if (string.IsNullOrWhiteSpace(ten))
            {
                string email = user.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    int at = email.IndexOf('@');
                    ten = (at > 0) ? email.Substring(0, at) : email;
                }
            }

            if (string.IsNullOrWhiteSpace(ten)) return "Người dùng";

            ten = Regex.Replace(ten.Trim(), "\\s+", " ");

            try
            {
                CultureInfo vi = new CultureInfo("vi-VN");
                ten = vi.TextInfo.ToTitleCase(ten.ToLower(vi));
            }
            catch
            {
                // ignore
            }

            return ten;
        }

        #endregion
    }
}
