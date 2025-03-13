using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.QuizQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;

namespace ZadElealm.Apis.Handlers.QuizHandler
{
    public class GetQuizQueryHandler : BaseQueryHandler<GetQuizQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetQuizQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetQuizQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new QuizWithQuestionsAndChoicesAndProgressSpecification(request.QuizId);
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecNoTrackingAsync(spec);

                if (quiz == null)
                    return new ApiResponse(404, "الاختبار غير موجود");

                var mappedQuiz = _mapper.Map<QuizResponseDto>(quiz);
                return new ApiDataResponse(200,mappedQuiz);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الاختبار");
            }
        }
    }
}
