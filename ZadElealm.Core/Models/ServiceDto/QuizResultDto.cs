namespace ZadElealm.Apis.Dtos
{
    public class QuizResultDto
    {
        public int Score { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
