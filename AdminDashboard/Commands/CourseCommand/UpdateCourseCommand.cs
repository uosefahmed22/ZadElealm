using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Core.Errors;

namespace AdminDashboard.Commands.CourseCommand
{
    public class UpdateCourseCommand : IRequest<bool>
    {
        public DashboardCourseDto CourseDto { get; set; }
    }
}
