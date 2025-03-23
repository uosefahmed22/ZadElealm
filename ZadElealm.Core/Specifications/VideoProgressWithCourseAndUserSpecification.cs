using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class VideoProgressWithCourseAndUserSpecification : BaseSpecification<VideoProgress>
    {
        public VideoProgressWithCourseAndUserSpecification(string userId, int courseId)
            : base(p => p.UserId == userId && p.CourseId == courseId)
        {
        }
    }
}