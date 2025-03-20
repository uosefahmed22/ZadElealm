using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class GetUserProfileQueryHandler : BaseQueryHandler<GetUserProfileQuery, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        public GetUserProfileQueryHandler(
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"user_profile_{request.UserId}";

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return new ApiResponse(401, "المستخدم غير موجود");

                var userDto = _mapper.Map<UserProfileDTO>(user);

                return new ApiDataResponse(200, userDto);
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب بيانات المستخدم");
            }
        }
    }
}
