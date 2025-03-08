using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries;

namespace ZadElealm.Apis.Commands.EnrollmentCommands
{
    // Enroll Course Command
    public class EnrollCourseCommand : BaseCommand<ApiResponse>
    {
        public int CourseId { get; }
        public string UserId { get; }

        public EnrollCourseCommand(int courseId, string userId)
        {
            CourseId = courseId;
            UserId = userId;
        }
    }
}
