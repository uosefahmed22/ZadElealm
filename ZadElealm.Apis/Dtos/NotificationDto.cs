using ZadElealm.Core.Enums;

namespace ZadElealm.Apis.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
