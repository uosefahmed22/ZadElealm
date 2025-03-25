using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Dto;
using AdminDashboard.Quires;
using AdminDashboard.Quires.CategoryQuery;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetAllCategoriesQuery();
            var categories = await _mediator.Send(query);
            return View(categories);
        }

        public IActionResult Create()
        {
            return View(new CreateCategoryDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var command = new CreateCategoryCommand
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.ImageUrl ?? null
            };

            var result = await _mediator.Send(command);

            if (result)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to create category");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var query = new GetCategoryByIdQuery { Id = id };
            var category = await _mediator.Send(query);

            var categoryDto = new CreateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
            };

            return View(categoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var command = new UpdateCategoryCommand
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.ImageUrl
            };

            var result = await _mediator.Send(command);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var query = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(query);

            if (result)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to delete category");
            return RedirectToAction("Index");
        }
    }
}
