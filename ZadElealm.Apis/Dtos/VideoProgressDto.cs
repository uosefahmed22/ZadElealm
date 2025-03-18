namespace ZadElealm.Apis.Dtos
{
    public class VideoProgressDto
    {
        public int VideoId { get; set; }
        public int CourseId { get; set; }
        public double WatchedDuration { get; set; }
        public bool IsCompleted { get; set; }
    }
}
