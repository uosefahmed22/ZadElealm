using MediatR;
using ZadElealm.Core.Enums;

namespace ZadElealm.Apis.Quaries.UserRankquery
{
    public class GetRankStatsQuery : IRequest<Dictionary<UserRankEnum, int>>
    {
    }
}
