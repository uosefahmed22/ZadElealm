using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Favorite
{
    public class FavoriteWithCourseAndUserSpecification : BaseSpecification<Core.Models.Favorite>
    {
        public FavoriteWithCourseAndUserSpecification(string userId)
            : base(f => f.UserId == userId)
        {
            Includes.Add(f => f.Course);
            OrderByDescending = f => f.CreatedAt;
        }
    }
}
