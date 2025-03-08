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
            : base(x => x.CourseId == courseId && x.UserId == userId)
        {
        }

        public EnrollmentSpecification(string userId)
            : base(x => x.UserId == userId)
        {
            Includes.Add(x => x.Course);
        }
    }
}
