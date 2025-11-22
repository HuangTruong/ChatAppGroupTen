using System;

namespace ChatApp.Models.Otp
{
    #region ThongTinMaFirebase
    /// <summary>
    /// Lưu trữ thông tin mã OTP trên Firebase.
    /// Firebase lưu dưới dạng object gồm:
    /// - Mã OTP (6 chữ số)
    /// - Thời điểm hết hạn (ISO-8601 dạng "o")
    /// </summary>
    public class ThongTinMaFirebase
    {
        /// <summary>
        /// Mã OTP được tạo (thường 6 số).
        /// </summary>
        public string Ma { get; set; }

        /// <summary>
        /// Thời điểm hết hạn OTP theo định dạng ISO8601 "o".
        /// Ví dụ: 2025-11-20T10:15:30.0000000Z
        /// </summary>
        public string HetHanLuc { get; set; }
    }
    #endregion
}
