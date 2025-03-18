using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Videos
{
    public class VideoProgressSpecification : BaseSpecification<VideoProgress>
    {
        public VideoProgressSpecification(string userId, int videoId)
            : base(p => p.UserId == userId && p.VideoId == videoId)
        {
            Includes.Add(p => p.Video);
            Includes.Add(p => p.Course);
        }
    }
}
