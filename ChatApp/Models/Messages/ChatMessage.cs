using System;

namespace ChatApp.Models.Messages
{
    /// <summary>
    /// Mô tả 1 tin nhắn trong cuộc trò chuyện (1-1 hoặc nhóm).
    /// Lưu ý:
    /// - MessageId: push key Firebase.
    /// - MessageType: "text" / "file".
    /// </summary>
    public class ChatMessage
    {
        #region ====== CORE FIELDS ======

        /// <summary>
        /// Key Firebase của message (Push key).
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// localId người gửi (đã SafeKey trước khi lưu).
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// localId người nhận (1-1) hoặc groupId (nhóm).
        /// </summary>
        public string ReceiverId { get; set; }

        /// <summary>
        /// Nội dung tin nhắn (text). Với messageType="file" có thể để rỗng.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Thời điểm gửi (Unix time milliseconds).
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Có phải tin nhắn của user hiện tại (phục vụ UI).
        /// </summary>
        public bool IsMine { get; set; }

        #endregion

        #region ====== FILE MESSAGE ======

        /// <summary>
        /// "text" hoặc "file".
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Tên file để hiển thị (vd: report.zip).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Dung lượng file (bytes).
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Link tải file (host trung gian).
        /// </summary>
        public string FileUrl { get; set; }

        #endregion

        #region ====== CTOR ======

        public ChatMessage()
        {
            MessageType = "text";
        }

        #endregion
    }
}
