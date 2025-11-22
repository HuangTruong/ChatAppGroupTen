using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChatApp.Services.Email
{
    #region SmtpEmailSender
    /// <summary>
    /// Triển khai giao diện <see cref="IEmailSender"/> để gửi email bằng SMTP.
    /// Mặc định gửi qua SMTP của Gmail.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        #region SMTP Configuration
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const bool EnableSsl = true;

        /// <summary>
        /// Email dùng để gửi.
        /// </summary>
        private const string FromEmail = "hnhom17@gmail.com";

        /// <summary>
        /// Tên hiển thị khi gửi email.
        /// </summary>
        private const string FromDisplayName = "ChatApp";

        /// <summary>
        /// App Password (Google Account → App passwords).
        /// </summary>
        private const string FromPasswordApp = "gcgq xzja ivub klbo";
        #endregion

        #region SendEmailAsync
        /// <summary>
        /// Gửi email HTML bất đồng bộ qua SMTP.
        /// </summary>
        /// <param name="toEmail">Email người nhận.</param>
        /// <param name="subject">Tiêu đề email.</param>
        /// <param name="htmlBody">Nội dung email dạng HTML.</param>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email nhận không hợp lệ.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                subject = "(No subject)";

            if (htmlBody == null)
                htmlBody = "";

            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.EnableSsl = EnableSsl;
                client.Credentials = new NetworkCredential(FromEmail, FromPasswordApp);

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(FromEmail, FromDisplayName);
                    msg.To.Add(new MailAddress(toEmail));
                    msg.Subject = subject;
                    msg.Body = htmlBody;
                    msg.IsBodyHtml = true;

                    try
                    {
                        await client.SendMailAsync(msg);
                    }
                    catch (Exception ex)
                    {
                        // Ném lỗi rõ ràng để Form/Service gọi báo lỗi đúng
                        throw new InvalidOperationException(
                            "Gửi email thất bại: " + ex.Message, ex);
                    }
                }
            }
        }
        #endregion
    }
    #endregion
}
