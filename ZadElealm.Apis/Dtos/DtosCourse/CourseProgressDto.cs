namespace ZadElealm.Apis.Dtos.DtosCourse
{
    public class CourseProgressDto
    {
        public float VideoProgress { get; set; }
        public float OverallProgress { get; set; }
        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
        public int RemainingVideos { get; set; }
        public bool IsEligibleForQuiz { get; set; }
    }
}
