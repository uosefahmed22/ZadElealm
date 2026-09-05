using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Favorite
{
    public class FavoriteSpecification : BaseSpecification<Core.Models.Favorite>
    {
        public FavoriteSpecification(string userId, int courseId)
            : base(f => f.AppUserId == userId && f.CourseId == courseId)
        {
        }

        public FavoriteSpecification(string userId)
         : base(f => f.AppUserId == userId)
        {
            AddThenInclude(query => query
                .Include(favorite => favorite.Course)
                .ThenInclude(course => course.Category));

            OrderByDescending = f => f.CreatedAt;
        }
    }
}
