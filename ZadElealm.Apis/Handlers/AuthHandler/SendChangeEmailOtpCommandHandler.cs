using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
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
            try
            {
                var user = await _userManager.FindByEmailAsync(request.NewEmail);
                if (user != null)
                {
                    return new ApiResponse(400, "Email already owned by another user");
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
                    Subject = "Change Email OTP",
                    Body = $"Your OTP is {otp}. It will expire in 5 minutes. Do not share it with anyone."
                };

                var result = await _sendEmailService.SendEmailAsync(emailMessage);
                if (result.StatusCode != 200)
                {
                    return new ApiResponse(400, "Failed to send OTP");
                }

                return new ApiResponse(200, "OTP sent successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An error occurred");
            }
        }
    }
}