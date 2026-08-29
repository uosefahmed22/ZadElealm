using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class ResetPasswordCommand : BaseCommand<ApiResponse>
    {
        public ResetPasswordDTO ResetPasswordDto { get; }

        public ResetPasswordCommand(ResetPasswordDTO resetPasswordDto)
        {
            ResetPasswordDto = resetPasswordDto;
        }
    }
}
