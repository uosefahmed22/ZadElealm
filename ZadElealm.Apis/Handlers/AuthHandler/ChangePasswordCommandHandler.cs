using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ChangePasswordCommandHandler : BaseCommandHandler<ChangePasswordCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public ChangePasswordCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            var result = await _userManager.ChangePasswordAsync(user,request.ChangePasswordDto.CurrentPassword,request.ChangePasswordDto.NewPassword);

            if (!result.Succeeded)
                return new ApiResponse(400, "كلمة المرور الحالية غير صحيحة");

            return new ApiResponse(200, "كلمة المرور تم تغييرها بنجاح");
        }
    }
}
