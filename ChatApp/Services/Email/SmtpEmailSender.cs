using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChatApp.Services.Email
{
    public class SmtpEmailSender
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const bool EnableSsl = true;

        // ĐỔI 3 DÒNG NÀY THEO TÀI KHOẢN GỬI MAIL CỦA BẠN
        private const string FromEmail = "hnhom17@gmail.com";
        private const string FromDisplayName = "ChatApp";
        private const string FromPasswordApp = "gcgq xzja ivub klbo"; // App Password

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.EnableSsl = EnableSsl;
                client.Credentials = new NetworkCredential(FromEmail, FromPasswordApp);

                var msg = new MailMessage();
                msg.From = new MailAddress(FromEmail, FromDisplayName);
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.Body = htmlBody;

                await client.SendMailAsync(msg);
            }
        }
    }
}
