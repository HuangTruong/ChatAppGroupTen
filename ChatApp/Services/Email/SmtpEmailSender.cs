using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChatApp.Services.Email
{
    /// <summary>
    /// Dịch vụ gửi email qua SMTP (Gmail).
    /// Dùng để gửi các email hệ thống như mã xác nhận đăng ký, reset mật khẩu, v.v.
    /// </summary>
    public class SmtpEmailSender
    {
        #region ====== SMTP CONFIGURATION ======

        /// <summary>
        /// Địa chỉ máy chủ SMTP (Gmail).
        /// </summary>
        private const string SmtpHost = "smtp.gmail.com";

        /// <summary>
        /// Cổng SMTP sử dụng TLS (STARTTLS).
        /// </summary>
        private const int SmtpPort = 587;

        /// <summary>
        /// Bật mã hóa SSL/TLS cho kết nối SMTP.
        /// </summary>
        private const bool EnableSsl = true;

        #endregion

        #region ====== TÀI KHOẢN NGƯỜI GỬI ======

        /// <summary>
        /// Địa chỉ email dùng để gửi (nên là tài khoản chuyên dùng cho ứng dụng).
        /// </summary>
        private const string FromEmail = "hnhom17@gmail.com";

        /// <summary>
        /// Tên hiển thị của người gửi trong email.
        /// </summary>
        private const string FromDisplayName = "ChatApp";

        /// <summary>
        /// App Password của tài khoản Gmail.
        /// LƯU Ý: Không nên hard-code trong source khi đưa lên GitHub,
        /// nên đọc từ file cấu hình / biến môi trường.
        /// </summary>
        private const string FromPasswordApp = "gcgq xzja ivub klbo"; // App Password

        #endregion

        #region ====== GỬI MAIL ======

        /// <summary>
        /// Gửi email HTML bất đồng bộ.
        /// </summary>
        /// <param name="toEmail">Địa chỉ email người nhận.</param>
        /// <param name="subject">Tiêu đề email.</param>
        /// <param name="htmlBody">Nội dung email dạng HTML.</param>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.EnableSsl = EnableSsl;
                client.Credentials = new NetworkCredential(FromEmail, FromPasswordApp);

                var msg = new MailMessage
                {
                    From = new MailAddress(FromEmail, FromDisplayName),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = htmlBody
                };

                msg.To.Add(new MailAddress(toEmail));

                await client.SendMailAsync(msg);
            }
        }

        #endregion
    }
}
