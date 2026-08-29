using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Category
{
    public class GetCategoriesQueryHandler : BaseQueryHandler<GetCategoriesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Repository<ZadElealm.Core.Models.Category>().GetAllWithNoTrackingAsync();
            var mappedCategories = categories.ToDtos();

            return new ApiDataResponse(200, mappedCategories);
        }
    }
}