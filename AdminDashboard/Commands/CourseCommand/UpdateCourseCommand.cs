using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.CourseCommand
{
    public class UpdateCourseCommand : IRequest<ApiDataResponse>
    {
        public int Id { get; set; }
        public DashboardCourseDto CourseDto { get; set; }
        public UpdateCourseCommand(int id, DashboardCourseDto courseDto)
        {
            Id = id;
            CourseDto = courseDto;
        }
    }
}
