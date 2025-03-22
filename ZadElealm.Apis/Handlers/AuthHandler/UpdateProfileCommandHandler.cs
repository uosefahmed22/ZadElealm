using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class UpdateProfileCommandHandler : BaseCommandHandler<UpdateProfileCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISendEmailService _sendEmailService;

        public UpdateProfileCommandHandler(UserManager<AppUser> userManager,
            IOtpService otpService,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _sendEmailService = sendEmailService;
        }
        public override async Task<ApiResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            user.DisplayName = request.DisplayName;
            user.PhoneNumber = request.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ApiResponse(400, "فشل تحديث البيانات");
            }

            return new ApiResponse(200, "تم تحديث البيانات بنجاح");
        }
    }
}
