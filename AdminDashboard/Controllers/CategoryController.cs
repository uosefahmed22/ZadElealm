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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryController(IMediator mediator,
            IUnitOfWork unitOfWork, IMapper mapper)
        {
           _mediator = mediator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                ImageUrl = model.ImageUrl?? null
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

            if (category == null)
                return NotFound();

            var model = _mapper.Map<CreateCategoryDto>(category);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var command = _mapper.Map<UpdateCategoryCommand>(model);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return RedirectToAction("Index");
        }
       
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetEntityAsync(id);
            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}
