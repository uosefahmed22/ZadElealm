using MediatR;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.UserRankCommand
{
    public class CalculateUserPointsCommand : IRequest<ApiDataResponse>
    {
        public string UserId { get; set; }
    }
}
