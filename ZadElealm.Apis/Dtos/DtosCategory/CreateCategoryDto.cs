namespace ZadElealm.Apis.Dtos.DtosCategory
{
    public class CreateCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
