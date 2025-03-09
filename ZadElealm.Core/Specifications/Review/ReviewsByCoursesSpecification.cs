using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Review
{
    public class ReviewsByCoursesSpecification : BaseSpecification<Core.Models.Review>
    {
        public ReviewsByCoursesSpecification(int courseId)
            : base(r => r.courseId == courseId)
        {
            Includes.Add(r => r.User);
            AddOrderByDescending(r => r.CreatedAt);
        }
    }
}
