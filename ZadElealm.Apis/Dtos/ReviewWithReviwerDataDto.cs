namespace ZadElealm.Apis.Dtos
{
    public class ReviewWithReviwerDataDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CourseId { get; set; }
        public string AppUserId { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public bool HasReplies { get; set; }
        public int RepliesCount { get; set; }
        public int LikesCount { get; set; }
    }
}
