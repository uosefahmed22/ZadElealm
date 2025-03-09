using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Review
{
    public class ReviewSpecification : BaseSpecification<Core.Models.Review>
    {
        public ReviewSpecification(string userId, int courseId)
            : base(r => r.UserId == userId && r.courseId == courseId)
        {
        }
    }
}
