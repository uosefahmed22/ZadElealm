using MediatR;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.Review
{
    public class GetReviewRepliesQuery : BaseQuery<ApiResponse>
    {
        public int ReviewId { get; }
        public string UserId { get; }

        public GetReviewRepliesQuery(int reviewId, string userId)
        {
            ReviewId = reviewId;
            UserId = userId;
        }
    }
}
