namespace AdminDashboard.Dto
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
        public IFormFile? Image { get; set; }
    }
}
