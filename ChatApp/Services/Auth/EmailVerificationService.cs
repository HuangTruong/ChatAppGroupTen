using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Services.Email;


namespace ChatApp.Services.Auth
{
    public static class EmailVerificationService
    {
        private class Entry
        {
            public string Code { get; set; } = "";
            public DateTime ExpireAt { get; set; }
            public DateTime LastSentAt { get; set; }
            public int Attempts { get; set; } = 0;
        }

        private static readonly ConcurrentDictionary<string, Entry> _store = new ConcurrentDictionary<string, Entry>();

        private const int ExpireMinutes = 5;          // Mã sống 5 phút
        private const int ResendCooldownSeconds = 60; // 60s mới cho gửi lại
        private const int MaxAttempts = 10;           // Giới hạn thử sai

        private static string GenerateCode()
        {
            var bytes = new byte[4];

            // Tạo số ngẫu nhiên an toàn bằng RandomNumberGenerator cho .NET Framework
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            uint val = BitConverter.ToUInt32(bytes, 0) % 1000000u; // 0..999999
            return val.ToString("D6"); // format thành 6 số, đủ 0 phía trước
        }


        public static bool CanResend(string email, out int waitSeconds)
        {
            waitSeconds = 0;
            if (_store.TryGetValue(email, out var e))
            {
                var remain = (int)Math.Ceiling(
                    (e.LastSentAt.AddSeconds(ResendCooldownSeconds) - DateTime.UtcNow).TotalSeconds);
                if (remain > 0)
                {
                    waitSeconds = remain;
                    return false;
                }
            }
            return true;
        }

        public static async Task SendNewCodeAsync(string email)
        {
            var code = GenerateCode();

            _store.AddOrUpdate(email,
                _new => new Entry
                {
                    Code = code,
                    ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes),
                    LastSentAt = DateTime.UtcNow,
                    Attempts = 0
                },
                (_new, old) =>
                {
                    old.Code = code;
                    old.ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes);
                    old.LastSentAt = DateTime.UtcNow;
                    old.Attempts = 0;
                    return old;
                });

            var html = new StringBuilder()
                .Append("<div style='font-family:Segoe UI,Arial,sans-serif'>")
                .Append("<h2>Mã xác nhận đăng ký ChatApp</h2>")
                .Append("<p>Mã của bạn là:</p>")
                .Append($"<div style='font-size:26px;font-weight:bold;letter-spacing:3px'>{code}</div>")
                .Append("<p>Mã có hiệu lực trong 5 phút.</p>")
                .Append("</div>")
                .ToString();

            var sender = new SmtpEmailSender();
            await sender.SendEmailAsync(email, "Mã xác nhận đăng ký ChatApp", html);
        }

        public static bool Verify(string email, string code, out string error)
        {
            error = "";
            if (!_store.TryGetValue(email, out var e))
            {
                error = "Chưa gửi mã tới email này. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }
            if (DateTime.UtcNow > e.ExpireAt)
            {
                error = "Mã đã hết hạn. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }
            if (e.Attempts >= MaxAttempts)
            {
                error = "Thử quá nhiều lần. Vui lòng bấm 'Gửi lại mã'.";
                return false;
            }

            e.Attempts++;

            if (!string.Equals(e.Code, code?.Trim(), StringComparison.Ordinal))
            {
                error = "Mã không đúng.";
                return false;
            }

            _store.TryRemove(email, out _);
            return true;
        }
    }
}
