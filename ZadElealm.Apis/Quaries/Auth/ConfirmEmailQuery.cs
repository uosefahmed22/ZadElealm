using ZadElealm.Apis.Commands;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.Auth
{
    public class ConfirmEmailQuery : BaseQuery<ApiResponse>
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
