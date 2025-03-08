using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.EnrollmentQuery
{
    public class EnrollmentCourseQuery : BaseQuery<ApiResponse>
    {
        public int CourseId { get; }
       public string UserId { get; }
        public EnrollmentCourseQuery(int courseId, string userId)
        {
            CourseId = courseId;
            UserId = userId;
        }
    }
}
