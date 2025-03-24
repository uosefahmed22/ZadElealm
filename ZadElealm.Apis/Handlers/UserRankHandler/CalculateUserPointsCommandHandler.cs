using MediatR;
using ZadElealm.Apis.Commands.UserRankCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class CalculateUserPointsCommandHandler : IRequestHandler<CalculateUserPointsCommand, ApiDataResponse>
    {
        private readonly IUserRankCalculator _rankCalculator;

        public CalculateUserPointsCommandHandler(IUserRankCalculator rankCalculator)
        {
            _rankCalculator = rankCalculator;
        }

        public async Task<ApiDataResponse> Handle(CalculateUserPointsCommand request, CancellationToken cancellationToken)
        {
            var points = await _rankCalculator.CalculatePoints(request.UserId);

            return new ApiDataResponse(200, points, "User points calculated");
        }
    }
}
