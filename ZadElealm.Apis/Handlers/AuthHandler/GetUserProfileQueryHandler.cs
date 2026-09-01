using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class GetUserProfileQueryHandler : BaseQueryHandler<GetUserProfileQuery, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public GetUserProfileQueryHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task<ApiResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            return new ApiDataResponse(200, user.ToProfileDto());
        }
    }
}
