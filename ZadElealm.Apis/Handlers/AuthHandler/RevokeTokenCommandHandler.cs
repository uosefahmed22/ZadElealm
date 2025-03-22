using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class RevokeTokenCommandHandler : BaseCommandHandler<RevokeTokenCommand, ApiResponse>
    {
        private readonly ITokenService _tokenService;

        public RevokeTokenCommandHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public override async Task<ApiResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {

            var result = await _tokenService.RevokeToken(request.TokenRequest.Token, request.TokenRequest.RefreshToken);

            return new ApiDataResponse(200, result);
        }
    }
}