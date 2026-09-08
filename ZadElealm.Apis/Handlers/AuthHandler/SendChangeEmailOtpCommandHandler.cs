using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class SendChangeEmailOtpCommandHandler : BaseCommandHandler<SendChangeEmailOtpCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICheckPasswordService _checkPasswordService;
        private readonly IOtpService _otpService;
        private readonly ISendEmailService _sendEmailService;

        public SendChangeEmailOtpCommandHandler(
            UserManager<AppUser> userManager,
            ICheckPasswordService checkPasswordService,
            IOtpService otpService,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _checkPasswordService = checkPasswordService;
            _otpService = otpService;
            _sendEmailService = sendEmailService;
        }

        public override async Task<ApiResponse> Handle(SendChangeEmailOtpCommand request, CancellationToken cancellationToken)
        {
            if (string.Equals(request.OldEmail, request.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse(400, "البريد الجديد هو نفس بريدك الحالي. أدخل بريدًا مختلفًا");
            }

            var user = await _userManager.FindByEmailAsync(request.NewEmail);
            if (user != null)
            {
                return new ApiResponse(400, "البريد الإلكتروني موجود بالفعل");
            }

            var checkPassword = await _checkPasswordService.CheckPasswordAsync(request.OldEmail, request.password);
            if (checkPassword.StatusCode != 200)
            {
                return checkPassword;
            }

            var otp = _otpService.GenerateOtp(request.NewEmail);
            var emailMessage = new EmailMessage
            {
                To = request.NewEmail,
                Subject = "تغيير البريد الإلكتروني",
                Body = AccountEmailTemplates.ChangeEmailOtp(otp)
            };

            var result = await _sendEmailService.SendEmailAsync(emailMessage, cancellationToken);
            if (result.StatusCode != 200)
            {
                return new ApiResponse(
                    result.StatusCode,
                    result.Message ?? "تعذر إرسال رمز التحقق حاليًا");
            }

            return new ApiResponse(200, "لقد تم إرسال رمز التحقق بنجاح");
        }
    }
}
