using MediatR;
using ZadElealm.Apis.Commands.UserRankCommand;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications.UserRank;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class UpdateUserRankCommandHandler : IRequestHandler<UpdateUserRankCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRankCalculator _rankCalculator;

        public UpdateUserRankCommandHandler(
            IUnitOfWork unitOfWork,
            IUserRankCalculator rankCalculator)
        {
            _unitOfWork = unitOfWork;
            _rankCalculator = rankCalculator;
        }

        public async Task<bool> Handle(UpdateUserRankCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var points = await _rankCalculator.CalculatePoints(request.UserId);
                var rank = _rankCalculator.DetermineRank(points);

                var spec = new UserRankWithUserSpecification(request.UserId);
                var userRank = await _unitOfWork.Repository<UserRank>()
                    .GetEntityWithSpecAsync(spec);

                if (userRank == null)
                {
                    userRank = new UserRank
                    {
                        UserId = request.UserId,
                        TotalPoints = points,
                        Rank = rank,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<UserRank>().AddAsync(userRank);
                }
                else
                {
                    userRank.TotalPoints = points;
                    userRank.Rank = rank;
                    userRank.LastUpdated = DateTime.UtcNow;
                    _unitOfWork.Repository<UserRank>().Update(userRank);
                }

                await _unitOfWork.Complete();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
