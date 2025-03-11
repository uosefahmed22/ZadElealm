using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.AppServices
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendNotificationAsync(NotificationServiceDto notificationServiceDto)
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
                UserId = notificationServiceDto.UserId,
                Notification = notification,
                IsRead = false
            };
            notification.UserNotifications.Add(userNotification);
            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.Complete();
        }
    }
}