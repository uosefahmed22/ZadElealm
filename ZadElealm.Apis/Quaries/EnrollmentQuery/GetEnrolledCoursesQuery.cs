using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.EnrollmentQuery
{
    public class GetEnrolledCoursesQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetEnrolledCoursesQuery(string userId)
        {
            UserId = userId;
        }
    }
}
