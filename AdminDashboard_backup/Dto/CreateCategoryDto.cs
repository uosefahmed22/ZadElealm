namespace AdminDashboard.Dto
{
    public class CreateCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
