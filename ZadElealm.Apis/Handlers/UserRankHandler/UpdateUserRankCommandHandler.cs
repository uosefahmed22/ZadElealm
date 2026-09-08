using MediatR;
using ZadElealm.Apis.Commands.UserRankCommand;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class UpdateUserRankCommandHandler : IRequestHandler<UpdateUserRankCommand, bool>
    {
        private readonly IUserRankCalculator _rankCalculator;

        public UpdateUserRankCommandHandler(IUserRankCalculator rankCalculator)
        {
            _rankCalculator = rankCalculator;
        }

        public async Task<bool> Handle(UpdateUserRankCommand request, CancellationToken cancellationToken)
        {
            await _rankCalculator.CalculatePoints(request.UserId);
            return true;
        }
    }
}
