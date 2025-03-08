using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;
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
                var spec = new CategoryWithCoursesSpecification(request.SpecParams);
                var courses = await _unitOfWork.Repository<Core.Models.Course>().GetAllWithSpecAsync(spec);

                if (courses == null || !courses.Any())
                    return new ApiResponse(404, "لا توجد دورات في هذه الفئة");

                var coursesDto = _mapper.Map<IReadOnlyList<CourseDto>>(courses);

                var totalItems = await _unitOfWork.Repository<Core.Models.Course>()
                    .CountAsync(spec);

                var metaData = MetaData.Create(
                    totalItems,
                    request.SpecParams.PageNumber,
                    request.SpecParams.PageSize
                );

                return new PaginatedResponse<CourseDto>(
                    200,
                    coursesDto,
                    metaData,
                    "تم جلب الدورات بنجاح"
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الدورات");
            }
        }
    }
}
