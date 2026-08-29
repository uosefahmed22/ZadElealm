using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.Auth
{
    public class GetUserProfileQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetUserProfileQuery(string userId)
        {
            UserId = userId;
        }
    }
}
