using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Course;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Course;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.Course
{
    public class GetCourseWithAllDataQueryHandler : BaseQueryHandler<GetCourseWithAllDataQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCourseWithAllDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetCourseWithAllDataQuery request, CancellationToken cancellationToken)
        {
            var spec = new CourseWithAllDataSpecification(request.CourseId);
            var course = await _unitOfWork.Repository<Core.Models.Course>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var mappedCourse = _mapper.Map<CourseResponseWithAllDataDto>(course);

            if (!string.IsNullOrEmpty(request.UserId))
            {
                var specvideoProgress = new VideoProgressWithCourseAndUserSpecification(request.UserId, request.CourseId);
                var videoProgress = await _unitOfWork.Repository<VideoProgress>()
                    .GetAllWithSpecNoTrackingAsync(specvideoProgress);

                foreach (var video in mappedCourse. Videos)
                {
                    var progress = videoProgress.FirstOrDefault(vp => vp.VideoId == video.Id);
                    if (progress != null)
                    {
                        video.IsCompleted = progress.IsCompleted;
                        video.WatchedDuration = progress.WatchedDuration;
                    }
                }
            }
            return new ApiDataResponse(200, mappedCourse);
        }
    }
}