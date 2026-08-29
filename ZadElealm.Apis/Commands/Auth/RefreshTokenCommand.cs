using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class RefreshTokenCommand : BaseCommand<ApiResponse>
    {
        public TokenRequestDto TokenRequest { get; }

        public RefreshTokenCommand(TokenRequestDto tokenRequest)
        {
            TokenRequest = tokenRequest;
        }
    }
}
