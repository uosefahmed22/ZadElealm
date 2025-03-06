using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Auth
{
    public class GetCurrentUserQuery : BaseCommand<ApiResponse>
    {
        public string UserId { get; }

        public GetCurrentUserQuery(string userId)
        {
            UserId = userId;
        }
    }
}
