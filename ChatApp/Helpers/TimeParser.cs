using System;
using System.Globalization;

namespace ChatApp.Helpers
{
    public static class TimeParser
    {
        public static DateTime ToUtc(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return DateTime.UtcNow;

            if (long.TryParse(s, out var ms) && ms > 946684800000L && ms < 4102444800000L)
                return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;

            if (DateTimeOffset.TryParseExact(s, "o", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dtoExact))
                return dtoExact.UtcDateTime;

            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                return dto.UtcDateTime;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;

            return DateTime.UtcNow;
        }
    }
}
