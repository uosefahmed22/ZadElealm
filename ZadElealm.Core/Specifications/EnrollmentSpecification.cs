using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class EnrollmentSpecification : BaseSpecification<Enrollment>
    {
        public EnrollmentSpecification(int courseId, string userId)
            : base(x => x.CourseId == courseId && x.AppUserId == userId)
        {
            Includes.Add(x => x.Course);

            AddThenInclude(quwery=> quwery
                .Include(c => c.Course)
                .ThenInclude(c => c.Videos));
        }

        public EnrollmentSpecification(string userId)
            : base(x => x.AppUserId == userId)
        {
            Includes.Add(x => x.Course);
        }
    }
}
