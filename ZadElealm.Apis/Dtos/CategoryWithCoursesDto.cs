namespace ZadElealm.Apis.Dtos
{
    public class CategoryWithCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CourseDto> Courses { get; set; }
    }
}
