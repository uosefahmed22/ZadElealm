using Microsoft.Extensions.Caching.Hybrid;
using ZadElealm.Apis.Caching;
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
        private readonly HybridCache _cache;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var mappedCategories = await _cache.GetOrCreateAsync(
                CatalogCache.CategoriesKey,
                async _ =>
                {
                    var categories = await _unitOfWork.Repository<ZadElealm.Core.Models.Category>()
                        .GetAllWithNoTrackingAsync();
                    return categories.ToDtos();
                },
                CatalogCache.CategoriesOptions,
                [CatalogCache.CatalogTag, CatalogCache.CategoriesTag],
                cancellationToken);

            return new ApiDataResponse(200, mappedCategories);
        }
    }
}
