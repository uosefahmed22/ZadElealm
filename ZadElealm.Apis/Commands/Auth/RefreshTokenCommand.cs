using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;

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
