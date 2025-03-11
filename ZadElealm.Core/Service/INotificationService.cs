using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.ServiceDto;

namespace ZadElealm.Core.Service
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationServiceDto notificationServiceDto);
    }
}
