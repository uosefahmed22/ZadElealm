using MediatR;

namespace AdminDashboard.Commands.CourseCommand
{
    public class DeleteCourseCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
