using System.Net;
using System.Net.Mail;
using System.Text;

namespace WebListenMusic.Services
{
    /// <summary>
    /// Email Service s? d?ng SMTP ?? g?i email
    /// H? tr? Gmail, Outlook, và các SMTP server khác
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// G?i email qua SMTP
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                
                var smtpHost = smtpSettings["SmtpHost"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUser = smtpSettings["SmtpUser"];
                var smtpPass = smtpSettings["SmtpPass"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"] ?? "MusicListen";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                // Ki?m tra c?u hình
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogWarning("Email settings not configured. Email not sent to {Email}", toEmail);
                    _logger.LogInformation("Would send email to {Email} with subject: {Subject}", toEmail, subject);
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        /// <summary>
        /// G?i email reset password v?i template gi?ng UI web
        /// </summary>
        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Reset Your Password - MusicListen";
            
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Password - MusicListen</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #121212;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #121212; min-height: 100vh;'>
        <tr>
            <td align='center' style='padding: 40px 20px;'>
                <table width='100%' cellpadding='0' cellspacing='0' style='max-width: 420px;'>
                    
                    <!-- Main Card -->
                    <tr>
                        <td style='background-color: #1e1e1e; border-radius: 16px; border: 1px solid #2d2d2d; box-shadow: 0 4px 24px rgba(0,0,0,0.3);'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                
                                <!-- Logo -->
                                <tr>
                                    <td align='center' style='padding: 40px 40px 24px;'>
                                        <table cellpadding='0' cellspacing='0'>
                                            <tr>
                                                <td style='font-size: 32px; color: #1f6feb; vertical-align: middle;'>&#9835;</td>
                                                <td style='padding-left: 12px; font-size: 24px; font-weight: 700; color: #ffffff;'>MusicListen</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- Icon -->
                                <tr>
                                    <td align='center' style='padding: 0 40px 16px;'>
                                        <table cellpadding='0' cellspacing='0'>
                                            <tr>
                                                <td style='background-color: #1f6feb; width: 64px; height: 64px; border-radius: 50%; text-align: center; vertical-align: middle;'>
                                                    <span style='font-size: 28px; color: #ffffff;'>&#128274;</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- Title -->
                                <tr>
                                    <td align='center' style='padding: 0 40px 8px;'>
                                        <h1 style='margin: 0; font-size: 24px; font-weight: 600; color: #ffffff;'>Reset Your Password</h1>
                                    </td>
                                </tr>
                                
                                <!-- Subtitle -->
                                <tr>
                                    <td align='center' style='padding: 0 40px 24px;'>
                                        <p style='margin: 0; font-size: 14px; color: #a0a0a0; line-height: 1.5;'>
                                            We received a request to reset your password for your MusicListen account.
                                        </p>
                                    </td>
                                </tr>
                                
                                <!-- Info Box -->
                                <tr>
                                    <td style='padding: 0 40px 24px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #252525; border-radius: 8px; border: 1px solid #333333;'>
                                            <tr>
                                                <td style='padding: 16px;'>
                                                    <table cellpadding='0' cellspacing='0'>
                                                        <tr>
                                                            <td style='width: 24px; vertical-align: top; color: #1f6feb; font-size: 16px;'>&#9201;</td>
                                                            <td style='padding-left: 12px; color: #c0c0c0; font-size: 13px; line-height: 1.5;'>
                                                                This link will <span style='color: #ef4444; font-weight: 600;'>expire in 24 hours</span>. Please reset your password as soon as possible.
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- Button -->
                                <tr>
                                    <td align='center' style='padding: 0 40px 24px;'>
                                        <table cellpadding='0' cellspacing='0'>
                                            <tr>
                                                <td style='background-color: #1f6feb; border-radius: 8px;'>

                                                    <a href='{resetLink}' style='display: inline-block; padding: 14px 32px; font-size: 14px; font-weight: 600; color: #ffffff; text-decoration: none;'>
                                                        Reset Password
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- Warning Box -->
                                <tr>
                                    <td style='padding: 0 40px 24px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: rgba(239, 68, 68, 0.1); border-radius: 8px; border-left: 3px solid #ef4444;'>
                                            <tr>
                                                <td style='padding: 12px 16px;'>
                                                    <table cellpadding='0' cellspacing='0'>
                                                        <tr>
                                                            <td style='width: 20px; vertical-align: top; color: #ef4444; font-size: 14px;'>&#9888;</td>
                                                            <td style='padding-left: 10px; color: #a0a0a0; font-size: 12px; line-height: 1.5;'>
                                                                If you <span style='color: #ffffff; font-weight: 500;'>did not request</span> a password reset, please ignore this email.
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                
                                <!-- Divider -->
                                <tr>
                                    <td style='padding: 0 40px;'>
                                        <div style='height: 1px; background-color: #2d2d2d;'></div>
                                    </td>
                                </tr>
                                
                                <!-- Link Fallback -->
                                <tr>
                                    <td style='padding: 20px 40px 32px;'>
                                        <p style='margin: 0 0 8px; font-size: 11px; color: #707070;'>
                                            If the button doesn't work, copy and paste this link:
                                        </p>
                                        <p style='margin: 0; font-size: 10px; color: #1f6feb; word-break: break-all; background-color: #252525; padding: 10px 12px; border-radius: 6px; border: 1px solid #333333;'>
                                            {resetLink}
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td align='center' style='padding: 32px 20px 20px;'>
                            <p style='margin: 0 0 4px; font-size: 12px; color: #606060;'>
                                &copy; 2026 <span style='color: #808080;'>MusicListen</span>. All rights reserved.
                            </p>
                            <p style='margin: 0; font-size: 11px; color: #505050;'>
                                This is an automated email. Please do not reply.
                            </p>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
