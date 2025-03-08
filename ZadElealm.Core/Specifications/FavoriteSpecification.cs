using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class FavoriteSpecification : BaseSpecification<Favorite>
    {
        public FavoriteSpecification(string userId, int courseId)
            : base(f => f.UserId == userId && f.CourseId == courseId)
        {
        }
    }
}
