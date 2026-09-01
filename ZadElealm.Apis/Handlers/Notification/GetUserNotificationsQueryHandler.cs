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
                .GetAllWithSpecNoTrackingAsync(spec);

            var response = new NotificationsResponse
            {
                Notifications = notifications.ToDtos(),
                UnreadCount = notifications.Count(n => !n.IsRead),
                TotalCount = notifications.Count
            };

            return new ApiDataResponse(200, response);
        }
    }
}
