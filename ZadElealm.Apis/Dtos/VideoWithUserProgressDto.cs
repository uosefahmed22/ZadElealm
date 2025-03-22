﻿namespace ZadElealm.Apis.Dtos
{
    public class VideoWithUserProgressDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan VideoDuration { get; set; }
        public int OrderInCourse { get; set; }
        public bool IsCompleted { get; set; }
        public TimeSpan WatchedDuration { get; set; }
    }
}
