using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
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
        private readonly IMapper _mapper;

        public GetUserNotificationsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var spec = new UserNotificationWithNotificationSpecification(request.UserId);
            var notifications = await _unitOfWork.Repository<UserNotification>()
                .GetAllWithSpecAsync(spec);

            if (!notifications.Any())
                return new ApiResponse(200, "لا توجد إشعارات");

            var response = new NotificationsResponse
            {
                Notifications = _mapper.Map<IReadOnlyList<NotificationDto>>(notifications.Select(n => n.Notification)),
                UnreadCount = notifications.Count(n => !n.IsRead),
                TotalCount = notifications.Count
            };

            await UpdateNotificationsReadStatus(notifications);

            return new ApiDataResponse(200, response);
        }
    

        private async Task UpdateNotificationsReadStatus(IEnumerable<UserNotification> notifications)
        {
            var unreadNotifications = notifications.Where(n => !n.IsRead);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            if (unreadNotifications.Any())
            await _unitOfWork.Complete();
        }
    }
}
