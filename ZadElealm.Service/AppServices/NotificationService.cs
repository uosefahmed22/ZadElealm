using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Errors;
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
            if (notificationServiceDto == null || string.IsNullOrWhiteSpace(notificationServiceDto.UserId) ||
                string.IsNullOrWhiteSpace(notificationServiceDto.Title) ||
                string.IsNullOrWhiteSpace(notificationServiceDto.Description))
                return new ApiDataResponse(400, null, "بيانات الإشعار غير مكتملة");

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

            notification.UserNotifications.Add(userNotification);

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.Complete();

            return new ApiDataResponse(200, null, "تم إرسال الإشعار بنجاح");
        }
    }
}
