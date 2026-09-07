using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Policies;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class GetEnrolledCoursesQueryHandler : BaseQueryHandler<GetEnrolledCoursesQuery, ApiResponse>
    {
        private readonly IEnrollmentReadRepository _enrollmentReadRepository;

        public GetEnrolledCoursesQueryHandler(IEnrollmentReadRepository enrollmentReadRepository)
        {
            _enrollmentReadRepository = enrollmentReadRepository;
        }

        public override async Task<ApiResponse> Handle(GetEnrolledCoursesQuery request, CancellationToken cancellationToken)
        {
            var enrollments = await _enrollmentReadRepository
                .GetUserCoursesWithProgressAsync(request.UserId, cancellationToken);

            var response = new AllEnrollementData()
            {
                Courses = enrollments.Select(ToCourseDto).ToList(),
                Progress = enrollments.Select(ToProgressDto).ToList(),
                AllEnrolledCourses = enrollments.Count
            };

            return new ApiDataResponse(
                200,
                response,
                enrollments.Any() ? "تم جلب الدورات المسجلة بنجاح" : "لا توجد دورات مسجلة");
        }

        private static CourseDto ToCourseDto(EnrolledCourseProgressReadModel enrollment)
        {
            return new CourseDto
            {
                Id = enrollment.CourseId,
                Name = enrollment.CourseName,
                Description = enrollment.CourseDescription,
                Author = enrollment.Author,
                CourseLanguage = enrollment.CourseLanguage,
                CourseVideosCount = enrollment.CourseVideosCount,
                rating = enrollment.Rating,
                ImageUrl = enrollment.ImageUrl,
                CreatedAt = enrollment.CreatedAt,
                Category = new CategoryResponseDto
                {
                    Id = enrollment.CategoryId,
                    Name = enrollment.CategoryName,
                    Description = enrollment.CategoryDescription,
                    ImageUrl = enrollment.CategoryImageUrl
                }
            };
        }

        private static CourseProgressDto ToProgressDto(
            EnrolledCourseProgressReadModel enrollment)
        {
            var completedVideos = Math.Min(enrollment.CompletedVideos, enrollment.TotalVideos);
            var percentage = enrollment.TotalVideos > 0
                ? (float)completedVideos / enrollment.TotalVideos * 100
                : 0;

            return new CourseProgressDto
            {
                CourseId = enrollment.CourseId,
                VideoProgress = percentage,
                OverallProgress = percentage,
                CompletedVideos = completedVideos,
                TotalVideos = enrollment.TotalVideos,
                RemainingVideos = enrollment.TotalVideos - completedVideos,
                IsEligibleForQuiz = CourseCompletionPolicy.IsEligibleForAssessment(percentage)
            };
        }
    }
}
