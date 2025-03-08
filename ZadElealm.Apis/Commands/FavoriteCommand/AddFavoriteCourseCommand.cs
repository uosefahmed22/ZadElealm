using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.FavoriteCommand
{
    public class AddFavoriteCourseCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public int CourseId { get; }

        public AddFavoriteCourseCommand(string userId, int courseId)
        {
            UserId = userId;
            CourseId = courseId;
        }
    }
}
