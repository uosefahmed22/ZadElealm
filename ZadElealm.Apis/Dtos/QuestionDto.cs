namespace ZadElealm.Apis.Dtos
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<ChoiceDto> Choices { get; set; }
    }
}
