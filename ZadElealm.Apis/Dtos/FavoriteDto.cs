namespace ZadElealm.Apis.Dtos
{
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public CourseDto Course { get; set; }
    }
}
