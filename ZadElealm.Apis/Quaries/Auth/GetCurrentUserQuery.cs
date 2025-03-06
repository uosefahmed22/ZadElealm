using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Auth
{
    public class GetCurrentUserQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetCurrentUserQuery(string userId)
        {
            UserId = userId;
        }
    }
}
