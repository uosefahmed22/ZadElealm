using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Notification;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Notification;

namespace ZadElealm.Apis.Handlers.Notification
{
    public class GetNotificationByIdQueryHandler : BaseQueryHandler<GetNotificationByIdQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetNotificationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new UserNotificationSpecification(request.UserId, request.NotificationId);
            var userNotification = await _unitOfWork.Repository<UserNotification>()
                .GetEntityWithSpecAsync(spec);

            if (userNotification == null)
                return new ApiResponse(404, "الإشعار غير موجود");

            var mappedNotification = _mapper.Map<NotificationDto>(userNotification.Notification);
            mappedNotification.IsRead = userNotification.IsRead;

            if (!userNotification.IsRead)
            {
                userNotification.IsRead = true;
                _unitOfWork.Repository<UserNotification>().Update(userNotification);
                var result = await _unitOfWork.Complete();

                if (result > 0)
                {
                    mappedNotification.IsRead = true;
                }
            }

            return new ApiDataResponse(200, mappedNotification);
        }
    }
}