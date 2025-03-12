namespace ZadElealm.Core.Models.ServiceDto
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CorrectChoice { get; set; }
        public List<ChoiceDto> Choices { get; set; }
    }
}
