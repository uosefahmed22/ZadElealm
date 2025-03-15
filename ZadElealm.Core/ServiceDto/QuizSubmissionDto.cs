namespace ZadElealm.Core.ServiceDto
{
    public class QuizSubmissionDto
    {
        public int QuizId { get; set; }
        public List<StudentAnswerDto> StudentAnswers { get; set; } = new List<StudentAnswerDto>();
    }
}
