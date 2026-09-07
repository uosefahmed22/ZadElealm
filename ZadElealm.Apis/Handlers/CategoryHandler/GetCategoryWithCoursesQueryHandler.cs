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
                var mixCategories = ShouldMixCategories(specParams);
                var spec = new CategoryWithCoursesSpecification(
                    specParams,
                    applyPagination: !mixCategories);
                var courses = await _unitOfWork.Repository<Core.Models.Course>()
                    .GetAllWithSpecNoTrackingAsync(spec);

                if (mixCategories)
                {
                    courses = MixCoursesByCategory(courses)
                        .Skip((specParams.PageNumber - 1) * specParams.PageSize)
                        .Take(specParams.PageSize)
                        .ToList();
                }

                coursesDto = courses.ToDtos();
            }

            return new CourseCatalogCacheEntry(coursesDto, totalItems);
        }

        private static bool ShouldMixCategories(CourseSpecParams specParams)
            => specParams.CategoryId <= 0
                && string.Equals(specParams.SortBy, "mixed", StringComparison.OrdinalIgnoreCase);

        private static IReadOnlyList<Core.Models.Course> MixCoursesByCategory(
            IEnumerable<Core.Models.Course> courses)
        {
            var categoryQueues = courses
                .GroupBy(course => course.CategoryId)
                .Select(group => new CategoryCourseQueue(
                    group.Key,
                    new Queue<Core.Models.Course>(group.OrderBy(course => ShuffleKey(course.Id)))))
                .ToList();
            var mixedCourses = new List<Core.Models.Course>();
            int? previousCategoryId = null;

            while (categoryQueues.Count > 0)
            {
                var nextCategory = categoryQueues
                    .Where(group => group.CategoryId != previousCategoryId)
                    .OrderByDescending(group => group.Courses.Count)
                    .ThenBy(group => ShuffleKey(group.CategoryId))
                    .FirstOrDefault()
                    ?? categoryQueues
                        .OrderByDescending(group => group.Courses.Count)
                        .ThenBy(group => ShuffleKey(group.CategoryId))
                        .First();

                mixedCourses.Add(nextCategory.Courses.Dequeue());
                previousCategoryId = nextCategory.CategoryId;
                if (nextCategory.Courses.Count == 0)
                {
                    categoryQueues.Remove(nextCategory);
                }
            }

            return mixedCourses;
        }

        private static int ShuffleKey(int value)
            => unchecked((value * 1103515245) + 12345);

        private sealed record CategoryCourseQueue(
            int CategoryId,
            Queue<Core.Models.Course> Courses);
    }
}
