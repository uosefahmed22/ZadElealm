using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Notification
{
    public class MarkAllNotificationsAsReadCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }

        public MarkAllNotificationsAsReadCommand(string userId)
        {
            UserId = userId;
        }
    }
}