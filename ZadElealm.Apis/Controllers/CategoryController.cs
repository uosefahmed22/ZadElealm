using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

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

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ApiResponse>> GetCategoryWithCourses(int categoryId)
        {
            var query = new GetCategoryWithCoursesQuery(categoryId);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }


    }
}
