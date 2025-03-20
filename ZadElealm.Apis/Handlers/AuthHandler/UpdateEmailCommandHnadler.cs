using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class UpdateEmailCommandHandler : BaseCommandHandler<UpdateEmailCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ISendEmailService _sendEmailService;

        public UpdateEmailCommandHandler(UserManager<AppUser> userManager,
            IOtpService otpService,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _sendEmailService = sendEmailService;
        }

        public override async Task<ApiResponse> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return new ApiResponse(404, "User not found");
                }

                var isValidOtp = _otpService.IsValidOtp(request.NewEmail, request.Token);
                if (!isValidOtp)
                {
                    return new ApiResponse(400, "Invalid or expired OTP");
                }

                var oldEmail = user.Email;

                var emailMessage = new EmailMessage
                {
                    To = oldEmail,
                    Subject = "Email Updated",
                    Body = "تم تحديث البريد الإلكتروني بنجاح, إذا لم تكن أنت من قام بتغيير البريد الإلكتروني يرجى التواصل بالدعم الفني عن طريق ارسال ريبورت"
                };
                await _sendEmailService.SendEmailAsync(emailMessage);

                var result = await _userManager.SetEmailAsync(user, request.NewEmail);
                if (!result.Succeeded)
                {
                    return new ApiResponse(400, "Failed to update email");
                }

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                return new ApiResponse(200, "Email updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An error occurred");
            }
        }
    }
}
