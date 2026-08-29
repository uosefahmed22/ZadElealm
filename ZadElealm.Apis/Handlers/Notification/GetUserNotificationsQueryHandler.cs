using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Notification;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Notification;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Handlers.Notification
{
    public class GetUserNotificationsQueryHandler : BaseQueryHandler<GetUserNotificationsQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var spec = new UserNotificationSpecification(request.UserId);
            var notifications = await _unitOfWork.Repository<UserNotification>()
                .GetAllWithSpecAsync(spec);

            if (!notifications.Any())
                return new ApiResponse(200, "لا توجد إشعارات");

            var response = new NotificationsResponse
            {
                Notifications = notifications.ToDtos(),
                UnreadCount = notifications.Count(n => !n.IsRead),
                TotalCount = notifications.Count
            };

            await UpdateNotificationsReadStatus(notifications);

            return new ApiDataResponse(200, response);
        }

        private async Task UpdateNotificationsReadStatus(IEnumerable<UserNotification> notifications)
        {
            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            if (unreadNotifications.Count > 0)
                await _unitOfWork.Complete();
        }
    }
}
