using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ForgetPasswordCommandHandler : BaseCommandHandler<ForgetPasswordCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ISendEmailService _sendEmailService;

        public ForgetPasswordCommandHandler(
            UserManager<AppUser> userManager,
            IOtpService otpService,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _sendEmailService = sendEmailService;
        }

        public override async Task<ApiResponse> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ApiResponse(400, "المستخدم غير موجود");
                }

                var otp = _otpService.GenerateOtp(request.Email);
                var emailMessage = CreateEmailMessage(request.Email, otp);

                await _sendEmailService.SendEmailAsync(emailMessage);

                return new ApiResponse(200, "الرمز المؤقت تم إرساله بنجاح.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ غير متوقع");
            }
        }

        private EmailMessage CreateEmailMessage(string email, string otp)
        {
            return new EmailMessage
            {
                To = email,
                Subject = "نسيت كلمة المرور",
                Body = $"<h1>الرمز المؤقت الخاص بك هو: {otp}</h1>"
            };
        }
    }
}
