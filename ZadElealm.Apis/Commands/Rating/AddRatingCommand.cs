using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Rating
{
    public class AddRatingCommand : BaseCommand<ApiResponse>
    {
        public int Value { get; }
        public int CourseId { get; }
        public string UserId { get; }

        public AddRatingCommand(int value, int courseId, string userId)
        {
            Value = value;
            CourseId = courseId;
            UserId = userId;
        }
    }
}
