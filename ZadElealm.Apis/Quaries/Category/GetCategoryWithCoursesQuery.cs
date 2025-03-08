using ZadElealm.Apis.Errors;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.Apis.Quaries.Category
{
    public class GetCategoryWithCoursesQuery : BaseQuery<ApiResponse>
    {
        public CourseSpecParams SpecParams { get; }

        public GetCategoryWithCoursesQuery(CourseSpecParams specParams)
        {
            SpecParams = specParams;
        }
    }
}
