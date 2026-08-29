using Microsoft.AspNetCore.Identity;
using System.Text;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ConfirmEmailQueryHandler : BaseQueryHandler<ConfirmEmailQuery, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public ConfirmEmailQueryHandler(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
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
                var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "https://zad-elealm.netlify.app";
                return new ApiDataResponse(
                    200,
                    "تم تأكيد البريد الإلكتروني بنجاح",
                    AuthUrlBuilder.BuildFrontendLoginUrl(frontendBaseUrl));
            }

            return new ApiResponse(400, "فشل في تأكيد البريد الإلكتروني. الرمز غير صالح أو منتهي الصلاحية");
        }
    }
}
