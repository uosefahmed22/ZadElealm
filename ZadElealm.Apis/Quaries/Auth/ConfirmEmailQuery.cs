using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Auth
{
    public class ConfirmEmailQuery : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public string Token { get; }  

        public ConfirmEmailQuery(string userId, string token)
        {
            UserId = userId;
            Token = token;
        }
    }
}
