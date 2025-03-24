using ZadElealm.Core.Enums;

namespace ZadElealm.Apis.Dtos
{
    public class UserRankDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public int TotalPoints { get; set; }
        public UserRankEnum Rank { get; set; }
        public string RankName => Rank.ToString();
        public int CompletedCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public double AverageQuizScore { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
