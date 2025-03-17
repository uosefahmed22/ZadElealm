using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public TestController(IQuizService quizService)
        {
            _quizService = quizService;
        }
        //create a quiz
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizDto quizDto)
        {
            var result = await _quizService.CreateQuizAsync(quizDto);
            return Ok(result);
        }
    }
}
