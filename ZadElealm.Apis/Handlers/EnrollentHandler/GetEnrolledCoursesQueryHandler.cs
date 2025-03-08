using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class GetEnrolledCoursesQueryHandler : BaseQueryHandler<GetEnrolledCoursesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetEnrolledCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(GetEnrolledCoursesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"enrolled_courses_{request.UserId}";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<CourseDto> cachedCourses))
                    return new ApiDataResponse(200, cachedCourses);

                var spec = new EnrollmentSpecification(request.UserId);
                var enrollments = await _unitOfWork.Repository<Enrollment>().GetAllWithSpecAsync(spec);

                if (!enrollments.Any())
                    return new ApiResponse(200, "لا توجد دورات مسجلة");

                var mappedCourses = _mapper.Map<IEnumerable<CourseDto>>(enrollments.Select(e => e.Course));

                var response = new AllEnrollementData()
                {
                    Courses = mappedCourses,
                    AllEnrolledCourses = enrollments.Count()
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {});
                _cache.Set(cacheKey, mappedCourses, cacheOptions);

                return new ApiDataResponse(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الدورات المسجلة");
            }
        }
    }
}



