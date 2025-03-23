using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Favorite;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Favorite;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Handlers.FavoriteHandlers
{
    public class GetFavoriteCoursesQueryHandler : BaseQueryHandler<GetFavoriteCoursesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFavoriteCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetFavoriteCoursesQuery request, CancellationToken cancellationToken)
        {
            var spec = new FavoriteSpecification(request.UserId);
            var favoriteCourses = await _unitOfWork.Repository<Favorite>()
                .GetAllWithSpecNoTrackingAsync(spec);

            if (!favoriteCourses.Any())
                return new ApiResponse(200, "لا توجد دورات مفضلة");

            var mappedCourses = _mapper.Map<IEnumerable<CourseDto>>(favoriteCourses.Select(e => e.Course));

            var response = new AllFavoriteCoursesData()
            {
                Courses = mappedCourses,
                AllFavoriteCourses = favoriteCourses.Count()
            };

            return new ApiDataResponse(200, response);
        }
    }
}