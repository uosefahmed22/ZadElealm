using AdminDashboard.Dto;
using AdminDashboard.Quires.CourseQuery;
using AutoMapper;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, IReadOnlyList<DashboardCourseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCoursesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IReadOnlyList<DashboardCourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses =await _unitOfWork.Repository<Course>().GetAllWithNoTrackingAsync();
            var mappedCourses = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<DashboardCourseDto>>(courses);
            return mappedCourses;
        }
    }
}
