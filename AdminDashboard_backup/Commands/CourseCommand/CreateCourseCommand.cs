using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.CourseCommand
{
    public class CreateCourseCommand : IRequest<ApiResponse>
    {
        public string PlaylistUrl { get; set; }
        public int CategoryId { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
    }
}
