using ZadElealm.Apis.Dtos.DtosCourse;

namespace ZadElealm.Apis.Dtos.DtosCategory
{
    public class CategoryWithCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CourseDto> Courses { get; set; }
    }
}
