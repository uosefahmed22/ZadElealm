using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Rating
{
    public class RatingSpecification : BaseSpecification<Core.Models.Rating>
    {
        public RatingSpecification(string userId, int courseId)
            : base(r => r.AppUserId == userId && r.courseId == courseId)
        {
        }
        public RatingSpecification(int courseId)
           : base(r => r.courseId == courseId)
        {
            AddOrderByDescending(r => r.CreatedAt);
        }
    }
}
