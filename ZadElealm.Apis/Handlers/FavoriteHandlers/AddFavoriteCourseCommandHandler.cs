using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Handlers.FavoriteHandlers
{
    public class AddFavoriteCourseCommandHandler : BaseCommandHandler<AddFavoriteCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public AddFavoriteCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(AddFavoriteCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _unitOfWork.Repository<Core.Models.Course>().GetByIdAsync(request.CourseId);
                if (course == null)
                    return new ApiResponse(404, "الدورة غير موجودة");

                var spec = new FavoriteWithCourseAndUserSpecification(request.UserId);
                var existingFavorite = await _unitOfWork.Repository<Favorite>()
                    .GetEntityWithSpecAsync(new FavoriteSpecification(request.UserId, request.CourseId));

                if (existingFavorite != null)
                    return new ApiResponse(400, "الدورة موجودة بالفعل في المفضلة");

                var favorite = new Favorite
                {
                    UserId = request.UserId,
                    CourseId = request.CourseId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Favorite>().AddAsync(favorite);
                await _unitOfWork.Complete();

                _cache.Remove($"favorite_courses_{request.UserId}");

                return new ApiResponse(200, "تمت إضافة الدورة إلى المفضلة بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إضافة الدورة إلى المفضلة");
            }
        }
    }
}
