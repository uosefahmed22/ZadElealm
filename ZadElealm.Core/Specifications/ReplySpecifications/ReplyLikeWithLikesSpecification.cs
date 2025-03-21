using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.ReplySpecifications
{
    public class ReplyLikeWithLikesSpecification : BaseSpecification<ReplyLike>
    {
        public ReplyLikeWithLikesSpecification(int replyId, string userId)
            : base(r => r.ReplyId == replyId && r.AppUserId == userId)
        {
        }
    }
}
