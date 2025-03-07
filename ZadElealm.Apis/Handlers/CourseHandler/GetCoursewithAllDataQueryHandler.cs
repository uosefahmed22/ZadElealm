using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Course;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.Course
{
    public class GetCourseWithAllDataQueryHandler : BaseQueryHandler<GetCourseWithAllDataQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetCourseWithAllDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(GetCourseWithAllDataQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"course_{request.CourseId}_full_data";

                if (_cache.TryGetValue(cacheKey, out CourseResponseWithAllDataDto cachedCourse))
                    return new ApiDataResponse(200, cachedCourse);

                var spec = new CourseWithAllDataSpecification(request.CourseId);
                var course = await _unitOfWork.Repository<ZadElealm.Core.Models.Course>().GetEntityWithSpecAsync(spec);

                if (course == null)
                    return new ApiResponse(404, "الدورة غير موجودة");

                var mappedCourse = _mapper.Map<CourseResponseWithAllDataDto>(course);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, mappedCourse, cacheOptions);

                return new ApiDataResponse(200,mappedCourse);
            }
            catch
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب بيانات الدورة");
            }
        }
    }
}
