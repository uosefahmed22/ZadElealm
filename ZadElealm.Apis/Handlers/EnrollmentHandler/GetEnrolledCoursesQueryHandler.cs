using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class GetEnrolledCoursesQueryHandler : BaseQueryHandler<GetEnrolledCoursesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEnrolledCoursesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GetEnrolledCoursesQuery request, CancellationToken cancellationToken)
        {
            var spec = new EnrollmentSpecification(request.UserId);
            var enrollments = await _unitOfWork.Repository<Enrollment>()
                .GetAllWithSpecNoTrackingAsync(spec);

            if (!enrollments.Any())
                return new ApiResponse(200, "لا توجد دورات مسجلة");

            var mappedCourses = enrollments.Select(e => e.Course).ToDtos();

            var response = new AllEnrollementData()
            {
                Courses = mappedCourses,
                AllEnrolledCourses = enrollments.Count()
            };

            return new ApiDataResponse(200, response);
        }
    }
}