using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Enums;

namespace ZadElealm.Core.ServiceDto
{
    public class NotificationServiceDto
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType Type { get; set; }
    }
}
