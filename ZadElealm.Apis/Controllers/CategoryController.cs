using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.Apis.Controllers
{
    public class CategoryController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetCategories()
        {
            var query = new GetCategoriesQuery();
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-courses-by-category")]
        public async Task<ActionResult<PaginatedResponse<CourseDto>>>GetCoursesByCategory([FromQuery] CourseSpecParams specParams)
        {
            var query = new GetCategoryWithCoursesQuery(specParams);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }
    }
}
