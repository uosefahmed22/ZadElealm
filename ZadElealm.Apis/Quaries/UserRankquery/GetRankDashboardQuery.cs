using MediatR;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.UserRankquery
{
    public sealed class GetRankDashboardQuery : IRequest<ApiDataResponse>
    {
        public string UserId { get; init; } = string.Empty;
        public int Take { get; init; } = 10;
    }
}
