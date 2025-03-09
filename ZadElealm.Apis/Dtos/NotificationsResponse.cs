namespace ZadElealm.Apis.Dtos
{
    public class NotificationsResponse
    {
        public IReadOnlyList<NotificationDto> Notifications { get; set; }
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
