using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class UserNotification : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
        public bool IsRead { get; set; }
    }
}
