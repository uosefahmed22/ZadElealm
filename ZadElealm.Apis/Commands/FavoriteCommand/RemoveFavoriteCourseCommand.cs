using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.FavoriteCommand
{
    public class RemoveFavoriteCourseCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public int CourseId { get; }

        public RemoveFavoriteCourseCommand(string userId, int courseId)
        {
            UserId = userId;
            CourseId = courseId;
        }
    }
}
