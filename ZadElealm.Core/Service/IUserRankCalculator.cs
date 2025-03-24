using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Enums;

namespace ZadElealm.Core.Service
{
    public interface IUserRankCalculator
    {
        Task<int> CalculatePoints(string userId);
        UserRankEnum DetermineRank(int points);
        Task<UserRankDto> GetUserRank(string userId);
    }
}
