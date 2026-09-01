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
using System.ComponentModel.DataAnnotations;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;
using ZadElealm.Core.Errors;

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
            if (emailMessage == null)
                return new ApiDataResponse(400, null, "بيانات البريد الإلكتروني مطلوبة.");

            if (string.IsNullOrWhiteSpace(emailMessage.To))
                return new ApiDataResponse(400, null, "إلى لا يمكن أن يكون فارغاً أو خالياً.");

            if (string.IsNullOrWhiteSpace(emailMessage.Subject))
                return new ApiDataResponse(400, null, "الموضوع لا يمكن أن يكون فارغاً أو خالياً.");

            if (string.IsNullOrWhiteSpace(emailMessage.Body))
                return new ApiDataResponse(400, null, "النص لا يمكن أن يكون فارغاً أو خالياً.");

            if (!new EmailAddressAttribute().IsValid(emailMessage.To) ||
                !MailboxAddress.TryParse(emailMessage.To, out var recipient))
                return new ApiDataResponse(400, null, "عنوان البريد الإلكتروني غير صالح.");

            if (string.IsNullOrWhiteSpace(_emailSettings.Email) ||
                string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) ||
                _emailSettings.Port <= 0)
                return new ApiDataResponse(500, null, "إعدادات البريد الإلكتروني غير مكتملة");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.DisplayedName, _emailSettings.Email));
            message.To.Add(recipient);
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = emailMessage.Body };

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                return new ApiDataResponse(200, null, "تم إرسال البريد الإلكتروني بنجاح");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return new ApiDataResponse(500, null, "حدث خطأ أثناء إرسال البريد الإلكتروني");
            }
        }
    }
}
