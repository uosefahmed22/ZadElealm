using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class Video : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan VideoDuration { get; set; }
        public int OrderInCourse { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
