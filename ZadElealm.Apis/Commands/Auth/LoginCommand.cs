using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Commands.Auth
{
    public class LoginCommand : BaseCommand<ApiResponse>
    {
        public LoginDTO LoginDto { get; }

        public LoginCommand(LoginDTO loginDto)
        {
            LoginDto = loginDto;
        }
    }
}
