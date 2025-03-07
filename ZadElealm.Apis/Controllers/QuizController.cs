using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Controllers
{
    public class QuizController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuizController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizze(int quizId)
        {
            var spec = new QuizWithQuestionsAndChoicesAndProgressSpecification(quizId);
            var quizzes = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);
            var mappedQuizzes = _mapper.Map<QuizResponseDto>(quizzes);
            return Ok(mappedQuizzes);
        }
    }
}
