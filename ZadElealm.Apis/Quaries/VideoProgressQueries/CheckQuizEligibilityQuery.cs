using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Quaries.VideoProgressQueries
{
    public class CheckQuizEligibilityQuery : BaseQuery<EligibilityResponse>
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
    }
}
