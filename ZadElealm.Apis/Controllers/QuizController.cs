using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.AppServices;

namespace ZadElealm.Apis.Controllers
{
    public class QuizController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IQuizService _quizService;
        private readonly UserManager<AppUser> _userManager;

        public QuizController(IUnitOfWork unitOfWork, IMapper mapper, IQuizService quizService,UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _quizService = quizService;
            _userManager = userManager;
        }

        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizze(int quizId)
        {
            var spec = new QuizWithQuestionsAndChoicesAndProgressSpecification(quizId);
            var quizzes = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);
            var mappedQuizzes = _mapper.Map<QuizResponseDto>(quizzes);
            return Ok(mappedQuizzes);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("submit")]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz(QuizSubmissionDto submission)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

                var result = await _quizService.SubmitQuizAsync(user.Id, submission);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }
    }
}
