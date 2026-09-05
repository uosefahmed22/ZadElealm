namespace ZadElealm.Apis.Dtos.DtosCourse
{
    public class AllEnrollementData
    {
        public IReadOnlyList<CourseDto> Courses { get; set; } = [];
        public IReadOnlyList<CourseProgressDto> Progress { get; set; } = [];
        public int AllEnrolledCourses { get; set; }
    }
}
