using System;
using System.Globalization;

namespace ChatApp.Helpers
{
    /// <summary>
    /// Helper chuyển đổi nhiều dạng chuỗi ngày–giờ khác nhau về dạng <see cref="DateTime"/> UTC.
    /// Hữu ích khi Firebase trả về timestamp theo nhiều format khác nhau (Unix ms, ISO8601, DateTime thường...).
    /// </summary>
    public static class TimeParser
    {
        #region ======== Parse chuỗi thời gian về UTC ========

        /// <summary>
        /// Chuyển chuỗi thời gian về <see cref="DateTime"/> dạng UTC.
        /// Hỗ trợ nhiều định dạng:
        /// - Unix time (milliseconds)
        /// - ISO8601 chuẩn ("o")
        /// - ISO/DateTime mặc định
        /// - DateTime thường
        /// Nếu parse thất bại → trả về <see cref="DateTime.UtcNow"/>.
        /// </summary>
        /// <param name="s">Chuỗi thời gian cần chuyển đổi.</param>
        /// <returns>Giá trị <see cref="DateTime"/> theo UTC.</returns>
        public static DateTime ToUtc(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return DateTime.UtcNow;

            // ===== 1) Unix milliseconds =====
            if (long.TryParse(s, out long ms) &&
                ms > 946684800000L &&     // năm 2000
                ms < 4102444800000L)      // năm 2100
            {
                try
                {
                    return DateTimeOffset
                        .FromUnixTimeMilliseconds(ms)
                        .UtcDateTime;
                }
                catch
                {
                    // ignore và tiếp tục thử các dạng khác
                }
            }

            // ===== 2) ISO8601 chuẩn: yyyy-MM-ddTHH:mm:ss.fffffffZ =====
            if (DateTimeOffset.TryParseExact(
                    s,
                    "o",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dtoExact))
            {
                return dtoExact.UtcDateTime;
            }

            // ===== 3) Các dạng ISO / DateTime thông dụng =====
            if (DateTimeOffset.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dto))
            {
                return dto.UtcDateTime;
            }

            // ===== 4) DateTime bình thường =====
            if (DateTime.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt))
            {
                return dt.ToUniversalTime();
            }

            // ===== 5) Thất bại → trả thời gian hiện tại =====
            return DateTime.UtcNow;
        }

        #endregion
    }
}
