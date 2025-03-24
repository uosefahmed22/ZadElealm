using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.UserRank
{
    public class UserRankWithUserSpecification : BaseSpecification<Core.Models.UserRank>
    {
        public UserRankWithUserSpecification(string userId) : base(x => x.UserId == userId)
        {
            Includes.Add(x => x.User);
        }
    }
}
