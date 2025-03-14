using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Favorite;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Handlers.FavoriteHandlers
{
    public class AddFavoriteCourseCommandHandler : BaseCommandHandler<AddFavoriteCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AddFavoriteCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(AddFavoriteCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _unitOfWork.Repository<Core.Models.Course>()
                    .GetEntityAsync(request.CourseId);
                if (course == null)
                    return new ApiResponse(404, "الدورة غير موجودة");

                var spec = new FavoriteWithCourseAndUserSpecification(request.AppUserId);
                var existingFavorite = await _unitOfWork.Repository<Favorite>()
                    .GetEntityWithSpecAsync(new FavoriteSpecification(request.AppUserId, request.CourseId));

                if (existingFavorite != null)
                    return new ApiResponse(400, "الدورة موجودة بالفعل في المفضلة");

                var favorite = new Favorite
                {
                    AppUserId = request.AppUserId,
                    CourseId = request.CourseId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Favorite>().AddAsync(favorite);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "تمت إضافة الدورة إلى المفضلة بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إضافة الدورة إلى المفضلة");
            }
        }
    }
}
