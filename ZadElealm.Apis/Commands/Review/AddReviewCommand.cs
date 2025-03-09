using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Review
{
    public class AddReviewCommand : BaseCommand<ApiResponse>
    {
        public string ReviewText { get; }
        public int CourseId { get; }
        public string UserId { get; }

        public AddReviewCommand(string reviewText, int courseId, string userId)
        {
            ReviewText = reviewText;
            CourseId = courseId;
            UserId = userId;
        }
    }
}
