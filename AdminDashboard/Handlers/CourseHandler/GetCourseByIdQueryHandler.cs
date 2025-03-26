using AdminDashboard.Dto;
using AdminDashboard.Quires.CourseQuery;
using AutoMapper;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, DashboardCourseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardCourseDto> Handle(GetCourseByIdQuery query, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(query.Id);
            if (course == null)
                return null;

            return new DashboardCourseDto
            {
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
            };
        }
    }
}