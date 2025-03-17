namespace ZadElealm.Core.ServiceDto
{
    public class QuestionDto
    {
        public string Text { get; set; }
        public int CorrectChoiceIndex { get; set; } //Index of the correct choice in the Choices list
        public int QuizId { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
    }
}
