using System.ComponentModel.DataAnnotations;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class UpdateEmailCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; set; }
        public string NewEmail { get; set; }
        public string Token { get; set; }
    }
}
