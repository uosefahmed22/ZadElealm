using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Core.Specifications.Notification
{
    public class UserNotificationWithNotificationSpecification : BaseSpecification<UserNotification>
    {
        public UserNotificationWithNotificationSpecification(string userId)
            : base(un => un.AppUserId == userId)
        {
            Includes.Add(un => un.Notification);
            AddOrderByDescending(un => un.Notification.CreatedAt);
        }
    }
}
