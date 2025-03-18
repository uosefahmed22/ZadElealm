using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Service
{
    public interface INotificationService
    {
        Task<ApiDataResponse> SendNotificationAsync(NotificationServiceDto notificationServiceDto);
    }
}
