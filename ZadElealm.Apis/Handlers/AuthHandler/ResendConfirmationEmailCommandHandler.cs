using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ResendConfirmationEmailCommandHandler : BaseCommandHandler<ResendConfirmationEmailCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISendEmailService _sendEmailService;
        private readonly EmailRateLimiter _rateLimiter;

        public ResendConfirmationEmailCommandHandler(
            UserManager<AppUser> userManager,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _sendEmailService = sendEmailService;
            _rateLimiter = new EmailRateLimiter();
        }

        public override async Task<ApiResponse> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            if (user.EmailConfirmed)
                return new ApiResponse(400, "البريد الإلكتروني مؤكد بالفعل");

            // Check rate limiting
            var (canSend, waitTime) = _rateLimiter.CanSendEmail(request.Email);
            if (!canSend)
            {
                var minutes = Math.Ceiling(waitTime.Value.TotalMinutes);
                var hours = Math.Ceiling(waitTime.Value.TotalHours);
                var days = Math.Ceiling(waitTime.Value.TotalDays);

                string waitMessage = waitTime.Value.TotalMinutes switch
                {
                    <= 60 => $"يرجى الانتظار {minutes} دقيقة قبل إعادة المحاولة",
                    <= 1440 => $"يرجى الانتظار {hours} ساعة قبل إعادة المحاولة",
                    _ => $"يرجى الانتظار {days} يوم قبل إعادة المحاولة"
                };

                return new ApiResponse(429, waitMessage);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = GenerateCallBackUrl(token, user.Id);

            var emailBody = BuildEmailBody(user.DisplayName, callbackUrl);

            await _sendEmailService.SendEmailAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "تأكيد البريد الإلكتروني",
                Body = emailBody
            });

            _rateLimiter.RecordAttempt(request.Email);

            return new ApiResponse(200, "تم إرسال رسالة التأكيد بنجاح");
        }

        private string BuildEmailBody(string displayName, string callbackUrl)
        {
            return $@"<h1>عزيزي {displayName}</h1>
                  هذا البريد الإلكتروني تم إرساله لتأكيد بريدك الإلكتروني
                  <p>لتأكيد بريدك الإلكتروني، اضغط على الرابط أدناه:</p>
                  <p><a href='{callbackUrl}'>اضغط هنا</a></p>";
        }

        private string GenerateCallBackUrl(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return string.Empty;
            }

            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var encodedUserId = WebUtility.UrlEncode(userId);

            return $"https://zadelealm.runasp.net/api/Account/confirm-email?userId={encodedUserId}&token={encodedToken}";
        }
    }
}