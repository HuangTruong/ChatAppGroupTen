using System;
using System.Globalization;

namespace ChatApp.Helpers
{
    public static class TimeParser
    {
        // Chuyển nhiều dạng chuỗi thời gian về UTC
        public static DateTime ToUtc(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return DateTime.UtcNow;

            // unix ms
            if (long.TryParse(s, out long ms) &&
                ms > 946684800000L && ms < 4102444800000L)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                }
                catch { }
            }

            // ISO8601 chuẩn
            if (DateTimeOffset.TryParseExact(
                    s,
                    "o",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dtoExact))
                return dtoExact.UtcDateTime;

            if (DateTimeOffset.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dto))
                return dto.UtcDateTime;

            if (DateTime.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt))
                return dt.ToUniversalTime();

            return DateTime.UtcNow;
        }
    }
}
