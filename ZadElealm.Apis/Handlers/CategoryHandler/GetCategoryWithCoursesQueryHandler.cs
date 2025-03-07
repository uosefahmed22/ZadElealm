using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.Category
{
    public class GetCategoryWithCoursesQueryHandler : BaseQueryHandler<GetCategoryWithCoursesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetCategoryWithCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(GetCategoryWithCoursesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"category_{request.CategoryId}_with_courses";

                if (_cache.TryGetValue(cacheKey, out CategoryWithCoursesDto cachedCategory))
                    return new ApiDataResponse(200,cachedCategory);

                var spec = new CategoryWithCoursesSpecification(request.CategoryId);
                var category = await _unitOfWork.Repository<ZadElealm.Core.Models.Category>().GetEntityWithSpecAsync(spec);

                if (category == null)
                    return new ApiResponse(404, "الفئة غير موجودة");

                var mappedCategory = _mapper.Map<CategoryWithCoursesDto>(category);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, mappedCategory, cacheOptions);

                return new ApiDataResponse(200, mappedCategory);
            }
            catch
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الفئة والدورات");
            }
        }
    }
}
