using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Review
{
    public class GetUserAddRateingBeforeQuery : BaseQuery<bool>
    {
        public int CourseId { get; set; }
        public string UserId { get; set; }
        public GetUserAddRateingBeforeQuery(int courseId, string userId)
        {
            CourseId = courseId;
            UserId = userId;
        }
    }
}
