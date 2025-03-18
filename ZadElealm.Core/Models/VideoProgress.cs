using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class VideoProgress : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int VideoId { get; set; }
        public Video Video { get; set; }
        public int CourseId { get; set; }  
        public Course Course { get; set; }
        public TimeSpan WatchedDuration { get; set; }
        public bool IsCompleted { get; set; }
    }
}
