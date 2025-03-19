using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Dtos
{
    public class CreateQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CorrectChoiceId { get; set; }
        public int QuizId { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
    }
}
