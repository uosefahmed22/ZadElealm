using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.VideoProgressQueries;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class CheckQuizEligibilityHandler : BaseQueryHandler<CheckQuizEligibilityQuery, EligibilityResponse>
    {
        private readonly IMediator _mediator;

        public CheckQuizEligibilityHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<EligibilityResponse> Handle(CheckQuizEligibilityQuery request, CancellationToken cancellationToken)
        {
            var progress = await _mediator.Send(new GetCourseProgressQuery
            {
                UserId = request.UserId,
                CourseId = request.CourseId
            });

            var isEligible = progress.OverallProgress >= 80;

            return new EligibilityResponse
            {
                IsEligible = isEligible,
                Message = isEligible
                    ? "You can now take the quiz!"
                    : "Complete at least 80% of the videos to unlock the quiz"
            };
        }
    }
}
