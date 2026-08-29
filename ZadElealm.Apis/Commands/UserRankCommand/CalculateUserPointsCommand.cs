using MediatR;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.UserRankCommand
{
    public class CalculateUserPointsCommand : IRequest<ApiDataResponse>
    {
        public string UserId { get; set; }
    }
}
