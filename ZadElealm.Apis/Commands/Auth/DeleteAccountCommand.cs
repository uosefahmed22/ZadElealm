using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class DeleteAccountCommand : BaseCommand<ApiResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public DeleteAccountCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
