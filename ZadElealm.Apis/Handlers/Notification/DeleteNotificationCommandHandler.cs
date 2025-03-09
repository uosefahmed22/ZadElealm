using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Notification;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Notification;

namespace ZadElealm.Apis.Handlers.Notification
{
    public class DeleteNotificationCommandHandler : BaseCommandHandler<DeleteNotificationCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public DeleteNotificationCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new UserNotificationSpecification(request.UserId, request.NotificationId);
                var notification = await _unitOfWork.Repository<UserNotification>()
                    .GetEntityWithSpecAsync(spec);

                if (notification == null)
                    return new ApiResponse(404, "الإشعار غير موجود");

                _unitOfWork.Repository<UserNotification>().Delete(notification);
                await _unitOfWork.Complete();

                _cache.Remove($"user_notifications_{request.UserId}");

                return new ApiResponse(200, "تم حذف الإشعار بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء حذف الإشعار");
            }
        }
    }
}
