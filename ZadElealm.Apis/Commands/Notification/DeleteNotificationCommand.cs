using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Notification
{
    public class DeleteNotificationCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public int NotificationId { get; }

        public DeleteNotificationCommand(string userId, int notificationId)
        {
            UserId = userId;
            NotificationId = notificationId;
        }
    }
}
