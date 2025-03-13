using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class RefreshTokenCommandHandler : BaseCommandHandler<RefreshTokenCommand, ApiResponse>
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public override async Task<ApiResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _tokenService.RefreshToken(request.TokenRequest.Token,request.TokenRequest.RefreshToken);

                return new ApiDataResponse(200, result );
            }
            catch
            {
                return new ApiResponse(400, "فشل في تحديث التوكن");
            }
        }
    }
}
