using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class RegisterCommand : BaseCommand<ApiResponse>
    {
        public RegisterDTO RegisterDto { get; }

        public RegisterCommand(RegisterDTO registerDto)
        {
            RegisterDto = registerDto;
        }
    }
}
