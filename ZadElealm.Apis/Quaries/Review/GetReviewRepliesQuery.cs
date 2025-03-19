using MediatR;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Review
{
    public class GetReviewRepliesQuery : BaseQuery<ApiResponse>
    {
        public int ReviewId { get; }

        public GetReviewRepliesQuery(int reviewId)
        {
            ReviewId = reviewId;
        }
    }
}
