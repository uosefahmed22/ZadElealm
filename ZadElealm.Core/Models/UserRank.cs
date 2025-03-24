using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class UserRank : BaseEntity
    {
        public string UserId { get; set; }
        public int TotalPoints { get; set; }
        public UserRankEnum Rank { get; set; } 
        public int CompletedCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public double AverageQuizScore { get; set; }
        public DateTime LastUpdated { get; set; }
        public AppUser User { get; set; }
    }
}
