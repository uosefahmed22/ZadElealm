using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.CourseCommand
{
    public class UpdateCourseCommand : IRequest<bool>
    {
        public DashboardCourseDto CourseDto { get; set; }
    }
}
