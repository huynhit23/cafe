using System.Net;
using System.Net.Mail;

namespace cafe.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For demo/simplicity, we log the email. 
            // In a real app, use MailKit or SmtpClient with credentials from appsettings.
            
            _logger.LogInformation("SIMULATING EMAIL SEND:");
            _logger.LogInformation($"TO: {email}");
            _logger.LogInformation($"SUBJECT: {subject}");
            _logger.LogInformation($"BODY: {htmlMessage}");

            // Optional: Implement real SMTP if requested, but for now simulation is safer for dev environment
            /*
            try
            {
                var smtpServer = _config["Email:SmtpServer"];
                var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
                var smtpUser = _config["Email:SmtpUser"];
                var smtpPass = _config["Email:SmtpPass"];

                var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser!),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FAILED TO SEND EMAIL: {ex.Message}");
            }
            */
            
            await Task.CompletedTask;
        }
    }
}
