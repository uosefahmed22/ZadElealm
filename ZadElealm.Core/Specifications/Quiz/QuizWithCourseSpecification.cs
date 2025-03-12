using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Quiz
{
    public class QuizWithCourseSpecification : BaseSpecification<Core.Models.Quiz>
    {
        public QuizWithCourseSpecification(int courseId) : base(x => x.CourseId == courseId)
        {
            Includes.Add(q => q.Course);
        }
    }
}
