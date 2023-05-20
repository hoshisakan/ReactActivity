using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;


namespace Infrastructure.Email
{
    public class EmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        //TODO: Send email using SendGrid, but can't send email to user mailbox, reason unknown.
        // public async Task SendEmailAsync(string userEmail, string emailSubject, string msg)
        // {
        //     // SendGridClient client = new SendGridClient(_config["SendGrid:ApiKey"]);
        //     SendGridClient client = new SendGridClient("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");
        //     SendGridMessage message = new SendGridMessage
        //     {
        //         // From = new EmailAddress(Configuration["SendGrid:FromEmail"], Configuration["SendGrid:FromName"]),
        //         From = new EmailAddress("hibuki1958@outlook.com", "trycatchlearn"),
                
        //         Subject = emailSubject,
        //         PlainTextContent = msg,
        //         HtmlContent = msg
        //     };
        //     message.AddTo(new EmailAddress(userEmail));
        //     message.SetClickTracking(false, false);

        //     _logger.LogInformation("Sending email to {userEmail} with subject {emailSubject}", userEmail, emailSubject);

        //     await client.SendEmailAsync(message);

        //     _logger.LogInformation("Email sent to {userEmail} with subject {emailSubject}", userEmail, emailSubject);
        // }

        //TODO: Send email using MailKit and Gmail SMTP
        public async Task SendEmailAsync(string userEmail, string emailSubject, string msg)
        {
            MimeMessage emailToSend = new MimeMessage();

            _logger.LogInformation("Sending email to {userEmail} with subject {emailSubject}", userEmail, emailSubject);

            emailToSend.From.Add(MailboxAddress.Parse(_config["SMTPGoogle:Username"]));
            emailToSend.To.Add(MailboxAddress.Parse(userEmail));
            emailToSend.Subject = emailSubject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){
                Text = msg
            };
            _logger.LogInformation("Email sent to {userEmail} with subject {emailSubject}", userEmail, emailSubject);
            //TODO Send email
            using (SmtpClient emailClient = new SmtpClient())
            {
                emailClient.Connect(
                    _config["SMTPGoogle:Host"],
                    _config.GetValue<int>("SMTPGoogle:Port"),
                    MailKit.Security.SecureSocketOptions.StartTls
                );
                emailClient.Authenticate(
                    _config["SMTPGoogle:Username"],
                    _config["SMTPGoogle:Password"]
                );
                await emailClient.SendAsync(emailToSend);
                await emailClient.DisconnectAsync(true);
            }
        }
    }
}