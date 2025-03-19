using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Review
{
    public class ReviewLikeSpecification : BaseSpecification<ReviewLike>
    {
        public ReviewLikeSpecification(int reviewId, string appUserId)
            : base(rl => rl.ReviewId == reviewId && rl.AppUserId == appUserId)
        {
        }
    }
}
