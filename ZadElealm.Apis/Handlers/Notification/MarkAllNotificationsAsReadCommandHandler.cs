﻿using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Notification;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Notification;

namespace ZadElealm.Apis.Handlers.Notification
{
    public class MarkAllNotificationsAsReadCommandHandler : BaseCommandHandler<MarkAllNotificationsAsReadCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        public MarkAllNotificationsAsReadCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var spec = new UserNotificationSpecification(request.UserId);
            var notifications = await _unitOfWork.Repository<UserNotification>()
                .GetAllWithSpecAsync(spec);

            if (!notifications.Any())
                return new ApiResponse(200, "لا توجد إشعارات لتحديثها");

            foreach (var notification in notifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
            }
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم تحديد جميع الإشعارات كمقروءة");
        }
    }
}