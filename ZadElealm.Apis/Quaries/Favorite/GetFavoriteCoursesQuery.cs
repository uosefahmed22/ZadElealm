using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Favorite
{
    public class GetFavoriteCoursesQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetFavoriteCoursesQuery(string userId)
        {
            UserId = userId;
        }
    }
}