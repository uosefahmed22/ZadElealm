using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Notification;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Notification;

namespace ZadElealm.Apis.Handlers.Notification
{
    public class MarkNotificationAsReadCommandHandler : BaseCommandHandler<MarkNotificationAsReadCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        public MarkNotificationAsReadCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new UserNotificationSpecification(request.UserId, request.NotificationId);
                var notification = await _unitOfWork.Repository<UserNotification>()
                    .GetEntityWithSpecAsync(spec);

                if (notification == null)
                    return new ApiResponse(404, "الإشعار غير موجود");

                if (notification.IsRead)
                    return new ApiResponse(200, "الإشعار مقروء بالفعل");

                notification.IsRead = true;
                _unitOfWork.Repository<UserNotification>().Update(notification);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "تم تحديد الإشعار كمقروء");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء تحديث الإشعار");
            }
        }
    }
}
