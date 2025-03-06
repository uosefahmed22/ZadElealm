using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class RevokeTokenCommand : BaseCommand<ApiResponse>
    {
        public TokenRequestDto TokenRequest { get; }

        public RevokeTokenCommand(TokenRequestDto tokenRequest)
        {
            TokenRequest = tokenRequest;
        }
    }
}
