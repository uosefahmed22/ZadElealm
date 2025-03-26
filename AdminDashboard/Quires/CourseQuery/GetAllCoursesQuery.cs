using AdminDashboard.Dto;
using MediatR;

namespace AdminDashboard.Quires.CourseQuery
{
    public class GetAllCoursesQuery : IRequest<IReadOnlyList<DashboardCourseDto>>
    {
    }
}
