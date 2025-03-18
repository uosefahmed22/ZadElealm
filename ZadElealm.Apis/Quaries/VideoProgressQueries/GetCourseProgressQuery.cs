using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Quaries.VideoProgressQueries
{
    public class GetCourseProgressQuery : BaseQuery<CourseProgressDto>
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
    }
}
