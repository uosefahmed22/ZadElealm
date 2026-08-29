using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class DeleteAccountCommandHandler : BaseCommandHandler<DeleteAccountCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteAccountCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public override async Task<ApiResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ApiResponse(404, "المستخدم غير موجود");
            }

            if (string.IsNullOrEmpty(request.Password) || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new ApiResponse(400, "كلمة المرور غير صحيحة");
            }

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse(400, "فشل حذف الحساب");
            }
            return new ApiResponse(200, "تم حذف الحساب بنجاح");
        }
    }
}
