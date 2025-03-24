using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class ProgressWithQuizSpecification : BaseSpecification<Progress>
    {
        public ProgressWithQuizSpecification(string userId, bool isCompleted) : base(x => x.AppUserId == userId && x.IsCompleted == isCompleted)
        {
            Includes.Add(x => x.Quiz);
            Includes.Add(x => x.Quiz.Course);
        }
    }
}
