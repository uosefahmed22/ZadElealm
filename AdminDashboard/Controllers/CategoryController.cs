﻿using AdminDashboard.Commands;
using AdminDashboard.Quires;
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
        private readonly IImageService _imageService;

        public CategoryController(IMediator mediator,
            IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
           _mediator = mediator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetAllCategoriesQuery();
            var categories = await _mediator.Send(query);
            return View(categories);
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
        public IActionResult Create()
        {
            var model = new CreateCategoryDto();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto model)
        {
            if(model.ImageUrl == null)
            {
                ModelState.AddModelError("ImageUrl", "Image is required!");
                return View(model);
            }

            var uploadedImage = await _imageService.UploadImageAsync(model.ImageUrl);
            var category = _mapper.Map<CreateCategoryDto, Category>(model);
            category.ImageUrl = uploadedImage;
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}
