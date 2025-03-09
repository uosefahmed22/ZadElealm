using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Specifications.Notification
{
    public class UserNotificationSpecification : BaseSpecification<UserNotification>
    {
        public UserNotificationSpecification(string userId, int notificationId)
            : base(un => un.UserId == userId && un.NotificationId == notificationId)
        {
            Includes.Add(un => un.Notification);
        }
    }
}
