using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Notification
{
    public class MarkNotificationAsReadCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public int NotificationId { get; }

        public MarkNotificationAsReadCommand(string userId, int notificationId)
        {
            UserId = userId;
            NotificationId = notificationId;
        }
    }
}
