using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Rating
{
    public class RatingsByCoursesSpecification : BaseSpecification<Core.Models.Rating>
    {
        public RatingsByCoursesSpecification(int courseId)
            : base(r => r.courseId == courseId)
        {
            AddOrderByDescending(r => r.CreatedAt);
        }
    }
}
