using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.QuizQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;

namespace ZadElealm.Apis.Handlers.QuizHandler
{
    public class GetQuizQueryHandler : BaseQueryHandler<GetQuizQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IVideoProgressService _videoProgressService;

        public GetQuizQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IVideoProgressService videoProgressService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _videoProgressService = videoProgressService;
        }

        public override async Task<ApiResponse> Handle(GetQuizQuery request, CancellationToken cancellationToken)
        {
            var spec = new QuizWithQuestionsAndChoicesAndProgressSpecification(request.QuizId);
            var quiz = await _unitOfWork.Repository<Quiz>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            if (quiz == null)
                return new ApiResponse(404, "الاختبار غير موجود");

            //var isUserEligible = await _videoProgressService.CheckCourseCompletionEligibilityAsync(request.UserId, quiz.CourseId);
            //if (!isUserEligible)
            //    return new ApiResponse(403, "غير مؤهل للدخول لهذا الاختبار");

            var mappedQuiz = _mapper.Map<QuizResponseDto>(quiz);
            return new ApiDataResponse(200, mappedQuiz);
        }
    }
}