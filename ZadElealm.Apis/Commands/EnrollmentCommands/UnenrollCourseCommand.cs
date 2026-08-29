using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.EnrollmentCommands
{
    public class UnenrollCourseCommand : BaseCommand<ApiResponse>
    {
        public int CourseId { get; }
        public string UserId { get; }

        public UnenrollCourseCommand(int courseId, string userId)
        {
            CourseId = courseId;
            UserId = userId;
        }
    }
}
