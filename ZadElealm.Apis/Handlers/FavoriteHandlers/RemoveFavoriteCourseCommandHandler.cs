using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Favorite;

namespace ZadElealm.Apis.Handlers.FavoriteHandlers
{
    public class RemoveFavoriteCourseCommandHandler : BaseCommandHandler<RemoveFavoriteCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveFavoriteCourseCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(RemoveFavoriteCourseCommand request, CancellationToken cancellationToken)
        {
            var favorite = await _unitOfWork.Repository<Favorite>()
                .GetEntityWithSpecAsync(new FavoriteSpecification(request.UserId, request.CourseId));

            if (favorite == null)
                return new ApiResponse(404, "الدورة غير موجودة في المفضلة");

            favorite.IsDeleted = true;
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم حذف الدورة من المفضلة بنجاح");
        }
    }
}