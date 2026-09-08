using Microsoft.Extensions.Caching.Hybrid;
using ZadElealm.Apis.Caching;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.Apis.Handlers.Category
{
    public class GetCategoryWithCoursesQueryHandler : BaseQueryHandler<GetCategoryWithCoursesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public GetCategoryWithCoursesQueryHandler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(GetCategoryWithCoursesQuery request, CancellationToken cancellationToken)
        {
            if (!IsSupportedSort(request.SpecParams.SortBy))
                return new ApiResponse(400, "قيمة ترتيب الدورات غير صالحة");

            var cacheKey = CatalogCache.GetCoursesKey(request.SpecParams);
            var cachedPage = await _cache.GetOrCreateAsync(
                cacheKey,
                async _ => await LoadCatalogPageAsync(request.SpecParams),
                CatalogCache.CoursesOptions,
                [CatalogCache.CatalogTag, CatalogCache.CoursesTag],
                cancellationToken);

            var metaData = new MetaData
            {
                CurrentPage = request.SpecParams.PageNumber,
                PageSize = request.SpecParams.PageSize,
                TotalMatchedItems = cachedPage.TotalItems,
                NumberOfPages = (int)Math.Ceiling(cachedPage.TotalItems / (double)request.SpecParams.PageSize)
            };

            metaData.NextPage = metaData.CurrentPage < metaData.NumberOfPages
                ? metaData.CurrentPage + 1
                : null;

            metaData.PreviousPage = metaData.CurrentPage > 1
                ? metaData.CurrentPage - 1
                : null;

            return new PaginatedResponse<CourseDto>(
                200,
                cachedPage.Courses,
                metaData,
                "تم جلب الدورات بنجاح"
            );
        }

        private async Task<CourseCatalogCacheEntry> LoadCatalogPageAsync(CourseSpecParams specParams)
        {
            var countSpec = new CategoryWithCoursesSpecification(specParams, true);
            var totalItems = await _unitOfWork.Repository<Core.Models.Course>().CountAsync(countSpec);

            IReadOnlyList<CourseDto> coursesDto = [];
            if (totalItems > 0)
            {
                var spec = new CategoryWithCoursesSpecification(specParams);
                var courses = await _unitOfWork.Repository<Core.Models.Course>()
                    .GetAllWithSpecNoTrackingAsync(spec);

                coursesDto = courses.ToDtos();
            }

            return new CourseCatalogCacheEntry(coursesDto, totalItems);
        }

        private static bool IsSupportedSort(string? sortBy)
            => string.IsNullOrWhiteSpace(sortBy) ||
                sortBy.Trim().ToLowerInvariant() is "rating" or "date" or "name" or "author";
    }
}
