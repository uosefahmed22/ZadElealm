using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class ChangePasswordCommand : BaseCommand<ApiResponse>
    {
        public string Email { get; }
        public ChangePasswordDTO ChangePasswordDto { get; }

        public ChangePasswordCommand(string email, ChangePasswordDTO changePasswordDto)
        {
            Email = email;
            ChangePasswordDto = changePasswordDto;
        }
    }
}
