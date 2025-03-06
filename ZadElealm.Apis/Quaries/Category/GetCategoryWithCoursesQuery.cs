using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Category
{
    public class GetCategoryWithCoursesQuery : BaseQuery<ApiResponse>
    {
        public int CategoryId { get; }
        public GetCategoryWithCoursesQuery(int categoryId)
        {
            CategoryId = categoryId;
        }
    }
}
