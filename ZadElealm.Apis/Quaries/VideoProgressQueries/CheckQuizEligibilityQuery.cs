using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Quaries.VideoProgressQueries
{
    public class CheckQuizEligibilityQuery : BaseQuery<ZadElealm.Core.Errors.ApiDataResponse>
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
    }
}
