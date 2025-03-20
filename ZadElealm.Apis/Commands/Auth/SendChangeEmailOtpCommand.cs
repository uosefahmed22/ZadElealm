using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class SendChangeEmailOtpCommand : BaseCommand<ApiResponse>
    {
        public string OldEmail { get; set; }
        public string password { get; set; }
        public string NewEmail { get; set; }
    }
}
