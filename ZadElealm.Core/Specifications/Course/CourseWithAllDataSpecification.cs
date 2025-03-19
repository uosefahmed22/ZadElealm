using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Course
{
    public class CourseWithAllDataSpecification : BaseSpecification<Core.Models.Course>
    {
        public CourseWithAllDataSpecification(int courseId)
            : base(x => x.Id == courseId)
        {
            Includes.Add(x => x.Category);
            Includes.Add(x => x.Videos.OrderBy(v => v.OrderInCourse));
            Includes.Add(x => x.Quizzes.OrderBy(q => q.CreatedAt));
            Includes.Add(x => x.enrollments);
            AddThenInclude(query => query
                .Include(c => c.Review)
                .ThenInclude(r => r.User));

            AddThenInclude(query => query
                .Include(c => c.Review)
                .ThenInclude(r => r.Replies));

        }
    }
}
