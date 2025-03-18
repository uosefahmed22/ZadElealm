using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Course
{
    public class CourseWithVideosAndQuizzesSpecification : BaseSpecification<Core.Models.Course>
    {
        public CourseWithVideosAndQuizzesSpecification(int courseId)
            : base(x => x.Id == courseId)
        {
            Includes.Add(c => c.Videos);
            Includes.Add(c => c.Quizzes);
        }
    }
}
