using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Videos
{
    public class VideoByIdSpecification : BaseSpecification<Video>
    {
        public VideoByIdSpecification(int videoId)
            : base(v => v.Id == videoId)
        {
        }
    }
}
