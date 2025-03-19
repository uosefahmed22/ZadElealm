using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Review
{
    public class DeleteReviewCommand : BaseCommand<ApiResponse>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public DeleteReviewCommand(int reviewId, string userId)
        {
            ReviewId = reviewId;
            UserId = userId;
        }
    }
}
