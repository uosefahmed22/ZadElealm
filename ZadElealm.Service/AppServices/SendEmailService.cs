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
using ZadElealm.Apis.Errors;

namespace ZadElealm.Service.AppServices
{
    public class SendEmailService : ISendEmailService
    {
        private readonly EmailSettings _emailSettings;
        public SendEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<ApiDataResponse> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emailMessage.To))
                return new ApiDataResponse(400, null, "إلى لا يمكن أن يكون فارغاً أو خالياً.");

            if (string.IsNullOrWhiteSpace(emailMessage.Subject))
                return new ApiDataResponse(400, null, "الموضوع لا يمكن أن يكون فارغاً أو خالياً.");

            if (string.IsNullOrWhiteSpace(emailMessage.Body))
                return new ApiDataResponse(400, null, "النص لا يمكن أن يكون فارغاً أو خالياً.");

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
                return new ApiDataResponse(200, null, "تم إرسال البريد الإلكتروني بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, "حدث خطأ أثناء إرسال البريد الإلكتروني");
            }
        }
    }
}
