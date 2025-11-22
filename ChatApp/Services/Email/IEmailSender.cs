using System.Threading.Tasks;

namespace ChatApp.Services.Email
{
    #region IEmailSender
    /// <summary>
    /// Interface cung cấp phương thức gửi email dạng HTML.
    /// Được dùng bởi OtpService, EmailVerificationService và các flow liên quan.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Gửi email bất đồng bộ.
        /// </summary>
        /// <param name="toEmail">
        /// Địa chỉ email người nhận.
        /// </param>
        /// <param name="subject">
        /// Tiêu đề email.
        /// </param>
        /// <param name="htmlBody">
        /// Nội dung HTML của email.
        /// </param>
        /// <returns>
        /// Task hoàn thành khi email gửi xong.
        /// </returns>
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
    #endregion
}
