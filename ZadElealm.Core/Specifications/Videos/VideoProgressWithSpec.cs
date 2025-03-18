using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Videos
{
    public class VideoProgressWithSpec : BaseSpecification<Models.VideoProgress>
    {
        public VideoProgressWithSpec(string userId, int courseId)
            : base(x => x.UserId == userId && x.Video.CourseId == courseId)
        {
            Includes.Add(p => p.Video);
        }
    }
}