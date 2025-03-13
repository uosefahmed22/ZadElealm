using AutoMapper;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var mappedCategories = _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<CategoryResponseDto>>(categories);
            return View(mappedCategories);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetEntityAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CreateCategoryDto
            {
                Name = category.Name,
                Description = category.Description,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoryExsist = await _unitOfWork.Repository<Category>().GetEntityAsync(model.Id);
            if (categoryExsist == null)
            {
                return NotFound();
            }

            categoryExsist.Name = model.Name;
            categoryExsist.Description = model.Description;

            if (model.ImageUrl != null)
            {
                var uploadedImage = await _imageService.UploadImageAsync(model.ImageUrl);
                categoryExsist.ImageUrl = uploadedImage;
            }

            await _unitOfWork.Complete();
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
