using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class VerifyOtpCommand : BaseCommand<ApiResponse>
    {
        public string Email { get; }
        public string Otp { get; }

        public VerifyOtpCommand(string email, string otp)
        {
            Email = email;
            Otp = otp;
        }
    }
}
