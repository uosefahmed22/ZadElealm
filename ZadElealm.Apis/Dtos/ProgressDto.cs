namespace ZadElealm.Apis.Dtos
{
    public class ProgressDto
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public int QuizId { get; set; }
        public string UserId { get; set; }
    }
}
