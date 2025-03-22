using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class GetCurrentUserQueryHandler : BaseQueryHandler<GetCurrentUserQuery, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public GetCurrentUserQueryHandler(
            UserManager<AppUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public override async Task<ApiResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            var userDto = await _tokenService.CreateToken(user);
            return new ApiDataResponse(200, userDto);
        }
    }
}
