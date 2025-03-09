using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType Type { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<UserNotification> UserNotifications { get; set; }
    }
}
