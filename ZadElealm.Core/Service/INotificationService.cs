using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Errors;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Service
{
    public interface INotificationService
    {
        Task<ApiDataResponse> AddNotificationAsync(NotificationServiceDto notificationServiceDto);
        Task<ApiDataResponse> SendNotificationAsync(NotificationServiceDto notificationServiceDto);
    }
}
