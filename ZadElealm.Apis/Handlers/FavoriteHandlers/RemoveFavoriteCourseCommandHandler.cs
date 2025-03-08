using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.FavoriteHandlers
{
    public class RemoveFavoriteCourseCommandHandler : BaseCommandHandler<RemoveFavoriteCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public RemoveFavoriteCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(RemoveFavoriteCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var favorite = await _unitOfWork.Repository<Favorite>()
                    .GetEntityWithSpecAsync(new FavoriteSpecification(request.UserId, request.CourseId));

                if (favorite == null)
                    return new ApiResponse(404, "الدورة غير موجودة في المفضلة");

                _unitOfWork.Repository<Favorite>().Delete(favorite);
                await _unitOfWork.Complete();

                _cache.Remove($"favorite_courses_{request.UserId}");

                return new ApiResponse(200, "تم حذف الدورة من المفضلة بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء حذف الدورة من المفضلة");
            }
        }
    }

}
