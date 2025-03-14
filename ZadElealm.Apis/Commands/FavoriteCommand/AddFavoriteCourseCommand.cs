using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.FavoriteCommand
{
    public class AddFavoriteCourseCommand : BaseCommand<ApiResponse>
    {
        public string AppUserId { get; }
        public int CourseId { get; }

        public AddFavoriteCourseCommand(string appUserId, int courseId)
        {
            AppUserId = appUserId;
            CourseId = courseId;
        }
    }
}
