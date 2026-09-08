using ZadElealm.Apis.Dtos.DtosCourse;

namespace ZadElealm.Apis.Quaries.VideoProgressQueries
{
    public class GetCourseProgressQuery : BaseQuery<ZadElealm.Core.Errors.ApiDataResponse>
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
    }
}
