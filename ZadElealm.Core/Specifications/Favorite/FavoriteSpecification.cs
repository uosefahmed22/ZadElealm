using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Favorite
{
    public class FavoriteSpecification : BaseSpecification<Core.Models.Favorite>
    {
        public FavoriteSpecification(string userId, int courseId)
            : base(f => f.AppUserId == userId && f.CourseId == courseId)
        {
        }
    }
}
