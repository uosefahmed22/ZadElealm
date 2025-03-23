using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Review
{
    public class RepliesWithUserSpecification : BaseSpecification<Reply>
    {
        public RepliesWithUserSpecification(int reviewId)
            : base(x => x.ReviewId == reviewId)
        {
            Includes.Add(x => x.User);
            Includes.Add(x => x.ReplyLikes);
        }
    }
}
