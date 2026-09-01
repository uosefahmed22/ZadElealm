using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications.UserRank;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public sealed class GetRankDashboardQueryHandler
        : IRequestHandler<GetRankDashboardQuery, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRankCalculator _rankCalculator;

        public GetRankDashboardQueryHandler(
            IUnitOfWork unitOfWork,
            IUserRankCalculator rankCalculator)
        {
            _unitOfWork = unitOfWork;
            _rankCalculator = rankCalculator;
        }

        public async Task<ApiDataResponse> Handle(
            GetRankDashboardQuery request,
            CancellationToken cancellationToken)
        {
            await _rankCalculator.CalculatePoints(request.UserId);

            var currentUser = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecNoTrackingAsync(new UserRankWithUserSpecification(request.UserId));

            if (currentUser == null)
            {
                return new ApiDataResponse(404, null, "تعذر العثور على تصنيف المستخدم");
            }

            var leaders = await _unitOfWork.Repository<UserRank>()
                .GetAllWithSpecNoTrackingAsync(new TopRankedUsersSpecification(request.Take));

            var response = new RankDashboardDto
            {
                CurrentUser = new StudentRankSummaryDto
                {
                    TotalPoints = currentUser.TotalPoints,
                    Rank = currentUser.Rank.ToString(),
                    CompletedCoursesCount = currentUser.CompletedCoursesCount,
                    CertificatesCount = currentUser.CertificatesCount,
                    AverageQuizScore = currentUser.AverageQuizScore,
                    LastUpdated = currentUser.LastUpdated
                },
                Leaders = leaders.Select((rank, index) => new LeaderboardEntryDto
                {
                    Position = index + 1,
                    DisplayName = string.IsNullOrWhiteSpace(rank.User?.DisplayName)
                        ? "طالب زاد العلم"
                        : rank.User.DisplayName,
                    ImageUrl = rank.User?.ImageUrl,
                    TotalPoints = rank.TotalPoints,
                    Rank = rank.Rank.ToString(),
                    CompletedCoursesCount = rank.CompletedCoursesCount,
                    IsCurrentUser = rank.UserId == request.UserId
                }).ToList()
            };

            return new ApiDataResponse(200, response);
        }
    }
}
