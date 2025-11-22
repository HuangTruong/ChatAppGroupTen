using System;

namespace ChatApp.Models.Chat
{
    #region TinNhan Model
    /// <summary>
    /// Mô hình dữ liệu tin nhắn dùng trong Firebase.
    /// Hỗ trợ chat 1-1 và chat nhóm. 
    /// Thời gian có thể là ISO8601 hoặc UnixTimeMilliseconds.
    /// </summary>
    public class TinNhan
    {
        /// <summary>
        /// ID duy nhất của tin nhắn trong Firebase.
        /// Dùng để tránh nhân đôi khi realtime.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Tên người gửi tin nhắn (tài khoản).
        /// </summary>
        public string guiBoi { get; set; }

        /// <summary>
        /// Tên người nhận (trong chat 1-1).
        /// Để trống (null hoặc "") nếu tin nhắn thuộc nhóm.
        /// </summary>
        public string nhanBoi { get; set; }

        /// <summary>
        /// Nội dung văn bản của tin nhắn.
        /// </summary>
        public string noiDung { get; set; }

        /// <summary>
        /// Thời điểm gửi tin:
        /// - Chuẩn ISO 8601 ("o")
        /// - Hoặc UnixTimeMilliseconds dạng chuỗi.
        /// (Được parse bằng TimeParser.ToUtc)
        /// </summary>
        public string thoiGian { get; set; }

        /// <summary>
        /// Xác định tin nhắn thuộc nhóm hay 1-1.
        /// True = nhóm, False = 1-1.
        /// </summary>
        public bool laNhom { get; set; }
    }
    #endregion
}
