using MediatR;
using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Quaries.UserRankquery
{
    public class GetTopRankedUsersQuery : IRequest<List<UserRankDto>>
    {
        public int Take { get; set; } = 10;
    }
}
