using Microsoft.AspNetCore.Identity;
using System.Text;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ConfirmEmailQueryHandler : BaseQueryHandler<ConfirmEmailQuery, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public ConfirmEmailQueryHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public override async Task<ApiResponse> Handle(ConfirmEmailQuery request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            if (user.EmailConfirmed)
                return new ApiResponse(400, "البريد الإلكتروني مؤكد بالفعل");

            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                return new ApiDataResponse(200, "تم تأكيد البريد الإلكتروني بنجاح", "https://zad-elealm.netlify.app/login");
            }

            return new ApiResponse(400, "فشل في تأكيد البريد الإلكتروني. الرمز غير صالح أو منتهي الصلاحية");
        }
    }
}