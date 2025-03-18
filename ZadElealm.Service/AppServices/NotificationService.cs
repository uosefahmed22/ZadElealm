using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Service.AppServices
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiDataResponse> SendNotificationAsync(NotificationServiceDto notificationServiceDto)
        {
            try
            {
                var notification = new Notification
                {
                    Title = notificationServiceDto.Title,
                    Description = notificationServiceDto.Description,
                    Type = notificationServiceDto.Type,
                    UserNotifications = new List<UserNotification>()
                };

                var userNotification = new UserNotification
                {
                    AppUserId = notificationServiceDto.UserId,
                    IsRead = false
                };

                if(notification.UserNotifications == null)
                    return new ApiDataResponse(400, null, "UserNotifications is null");

                notification.UserNotifications.Add(userNotification);

                await _unitOfWork.Repository<Notification>().AddAsync(notification);
                await _unitOfWork.Complete();
                return new ApiDataResponse(200, null, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, "Error while sending notification");
            }
        }
    }
}