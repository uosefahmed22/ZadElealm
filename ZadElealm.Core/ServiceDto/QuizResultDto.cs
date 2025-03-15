namespace ZadElealm.Core.ServiceDto
{
    public class QuizResultDto
    {
        public string QuizName { get; set; }
        public int Score { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int UnansweredQuestions { get; set; }
        public DateTime Date { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();
    }
}
