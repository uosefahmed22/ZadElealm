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

        public GetCategoryWithCoursesQueryHandler(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public override async Task<ApiResponse> Handle(GetCategoryWithCoursesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var countSpec = new CategoryWithCoursesSpecification(request.SpecParams, true);
                var totalItems = await _unitOfWork.Repository<Core.Models.Course>().CountAsync(countSpec);

                if (totalItems == 0)
                    return new ApiResponse(404, "لا توجد دورات في هذه الفئة");

                var spec = new CategoryWithCoursesSpecification(request.SpecParams);
                var courses = await _unitOfWork.Repository<Core.Models.Course>().GetAllWithSpecNoTrackingAsync(spec);

                var coursesDto = _mapper.Map<IReadOnlyList<CourseDto>>(courses);

                var metaData = new MetaData
                {
                    CurrentPage = request.SpecParams.PageNumber,
                    PageSize = request.SpecParams.PageSize,
                    TotalMatchedItems = totalItems,
                    NumberOfPages = (int)Math.Ceiling(totalItems / (double)request.SpecParams.PageSize)
                };

                metaData.NextPage = metaData.CurrentPage < metaData.NumberOfPages
                    ? metaData.CurrentPage + 1
                    : null;

                metaData.PreviousPage = metaData.CurrentPage > 1
                    ? metaData.CurrentPage - 1
                    : null;

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
