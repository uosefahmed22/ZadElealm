using AdminDashboard.Dto;
using MediatR;

namespace AdminDashboard.Quires.CourseQuery
{
    public class GetCourseByIdQuery : IRequest<DashboardCourseDto>
    {
        public int Id { get; set; }
    }
}
