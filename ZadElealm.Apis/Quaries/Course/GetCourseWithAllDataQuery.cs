using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Course
{
    public class GetCourseWithAllDataQuery : BaseQuery<ApiResponse>
    {
        public int CourseId { get; }

        public GetCourseWithAllDataQuery(int courseId)
        {
            CourseId = courseId;
        }
    }
}
