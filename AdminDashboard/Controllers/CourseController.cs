using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }
        public async Task<IActionResult> Index()
        {
            var courses =await _unitOfWork.Repository<Course>().GetAllAsync();
            var mappedCourses = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<CourseDto>>(courses);

            return View(mappedCourses);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new AdminDashboard.Dto.CourseDto
            {
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminDashboard.Dto.CourseDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courseExsist = await _unitOfWork.Repository<Course>().GetByIdAsync(model.Id);
            if (courseExsist == null)
            {
                return NotFound();
            }

            courseExsist.Name = model.Name;
            courseExsist.Description = model.Description;
            courseExsist.Author = model.Author;
            courseExsist.CourseLanguage = model.CourseLanguage;


            if (model.Image != null)
            {
                var uploadedImage = await _imageService.UploadImageAsync(model.Image);
                courseExsist.ImageUrl = uploadedImage;
            }

            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Repository<Course>().GetByIdAsync(id);
            _unitOfWork.Repository<Course>().Delete(category);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}
