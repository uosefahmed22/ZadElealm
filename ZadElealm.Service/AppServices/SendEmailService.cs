using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.AppServices
{
    public class SendEmailService : ISendEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SendEmailService> _logger;

        public SendEmailService(IOptions<EmailSettings> emailSettings, ILogger<SendEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emailMessage.To))
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(emailMessage.To));
            if (string.IsNullOrWhiteSpace(emailMessage.Subject))
                throw new ArgumentException("Subject cannot be null or empty.", nameof(emailMessage.Subject));
            if (string.IsNullOrWhiteSpace(emailMessage.Body))
                throw new ArgumentException("Body cannot be null or empty.", nameof(emailMessage.Body));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.DisplayedName, _emailSettings.Email));
            message.To.Add(new MailboxAddress("", emailMessage.To));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = emailMessage.Body };

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
                    await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, cancellationToken);
                    await client.SendAsync(message, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", emailMessage.To);
                throw new InvalidOperationException("Failed to send email.", ex);
            }
        }
    }
}
