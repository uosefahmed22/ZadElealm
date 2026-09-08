using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.UserRank
{
    public class TopRankedUsersSpecification : BaseSpecification<Core.Models.UserRank>
    {
        public TopRankedUsersSpecification(int take)
            : this(0, take)
        {
        }

        public TopRankedUsersSpecification(int skip, int take)
        {
            Includes.Add(x => x.User);
            AddOrderByDescending(x => x.TotalPoints);
            ApplyPagination(skip, take);
        }
    }
}
