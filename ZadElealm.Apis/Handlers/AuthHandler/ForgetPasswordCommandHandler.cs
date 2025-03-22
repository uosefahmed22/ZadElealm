using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ForgetPasswordCommandHandler : BaseCommandHandler<ForgetPasswordCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ISendEmailService _sendEmailService;
        private readonly EmailRateLimiter _rateLimiter;

        public ForgetPasswordCommandHandler(
            UserManager<AppUser> userManager,
            IOtpService otpService,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _sendEmailService = sendEmailService;
            _rateLimiter = new EmailRateLimiter();
        }

        public override async Task<ApiResponse> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ApiResponse(404, "المستخدم غير موجود");
            }

            var (canSend, waitTime) = _rateLimiter.CanSendEmail(request.Email);
            if (!canSend)
            {
                string waitMessage = FormatWaitTimeMessage(waitTime.Value);
                return new ApiResponse(429, waitMessage);
            }

            var otp = _otpService.GenerateOtp(request.Email);
            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "إعادة تعيين كلمة المرور",
                Body = BuildEmailBody(user.DisplayName, otp)
            };

            var emailSent = await _sendEmailService.SendEmailAsync(emailMessage);
            if (emailSent.StatusCode != 200)
            {
                return new ApiResponse(500, "فشل في إرسال البريد الإلكتروني");
            }

            _rateLimiter.RecordAttempt(request.Email);

            return new ApiResponse(200, "تم إرسال رمز التحقق بنجاح");
        }

        private string BuildEmailBody(string displayName, string otp)
        {
            return $@"<h1>عزيزي {displayName}</h1>
                  <p>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك.</p>
                  <p>رمز التحقق الخاص بك هو: <strong>{otp}</strong></p>
                  <p>هذا الرمز صالح لمدة خمسه عشر دقيقة.</p>
                  <p>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني.</p>";
        }
        private string FormatWaitTimeMessage(TimeSpan waitTime)
        {
            if (waitTime.TotalDays >= 1)
            {
                return $"يرجى الانتظار {Math.Ceiling(waitTime.TotalDays)} يوم قبل المحاولة مرة أخرى";
            }
            if (waitTime.TotalHours >= 1)
            {
                return $"يرجى الانتظار {Math.Ceiling(waitTime.TotalHours)} ساعة قبل المحاولة مرة أخرى";
            }
            return $"يرجى الانتظار {Math.Ceiling(waitTime.TotalMinutes)} دقيقة قبل المحاولة مرة أخرى";
        }
    }
}
