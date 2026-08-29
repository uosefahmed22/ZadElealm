using AdminDashboard.Dto;
using AdminDashboard.Mappers;
using AdminDashboard.Quires.CourseQuery;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, IReadOnlyList<DashboardCourseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCoursesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<DashboardCourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllWithNoTrackingAsync();
            return courses.ToDashboardDtos();
        }
    }
}
