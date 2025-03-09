using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Notification
{
    public class GetUserNotificationsQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetUserNotificationsQuery(string userId)
        {
            UserId = userId;
        }
    }
}
