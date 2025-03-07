using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Quaries.Course;

namespace ZadElealm.Apis.Controllers
{
    public class CourseController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseWithAllData(int courseId)
        {
            var query = new GetCourseWithAllDataQuery(courseId);
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }


    } 
}
