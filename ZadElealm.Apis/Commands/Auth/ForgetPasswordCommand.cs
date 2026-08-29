using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class ForgetPasswordCommand : BaseCommand<ApiResponse>
    {
        public string Email { get; }

        public ForgetPasswordCommand(string email)
        {
            Email = email;
        }
    }
}
