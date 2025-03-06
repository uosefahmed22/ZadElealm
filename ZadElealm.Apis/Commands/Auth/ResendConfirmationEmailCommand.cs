using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class ResendConfirmationEmailCommand : BaseCommand<ApiResponse>
    {
        public string Email { get; }

        public ResendConfirmationEmailCommand(string email)
        {
            Email = email;
        }
    }
}
