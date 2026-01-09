namespace WebListenMusic.Services
{
    /// <summary>
    /// Interface ??nh ngh?a các ph??ng th?c g?i email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// G?i email ??n gi?n
        /// </summary>
        /// <param name="toEmail">Email ng??i nh?n</param>
        /// <param name="subject">Tiêu ?? email</param>
        /// <param name="body">N?i dung email (HTML)</param>
        Task SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// G?i email reset password
        /// </summary>
        /// <param name="toEmail">Email ng??i nh?n</param>
        /// <param name="resetLink">Link reset password</param>
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
