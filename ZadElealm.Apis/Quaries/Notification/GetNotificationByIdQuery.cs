using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Notification
{
    public class GetNotificationByIdQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }
        public int NotificationId { get; }

        public GetNotificationByIdQuery(string userId, int notificationId)
        {
            UserId = userId;
            NotificationId = notificationId;
        }
    }
}
