using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
        private const int MaxRetries = 3;
        private const int MaxResultsPerPage = 50;

        public CourseController(IMediator mediator,
            IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _httpClient = httpClient;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _mediator.Send(new GetCategoriesQuery());
                ViewBag.Categories = categories;
                return View(new CourseViewModel());
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _mediator.Send(new GetCategoriesQuery());
                    ViewBag.Categories = categories;
                    return View(model);
                }

                var command = new CreateCourseCommand
                {
                    PlaylistUrl = model.PlaylistUrl,
                    CategoryId = model.CategoryId,
                    Author = model.Author,
                    CourseLanguage = model.CourseLanguage
                };

                var result = await _mediator.Send(command);

                if (result.StatusCode == 200)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", result.Message);
                var categoriesForError = await _mediator.Send(new GetCategoriesQuery());
                ViewBag.Categories = categoriesForError;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred.");
                var categories = await _mediator.Send(new GetCategoriesQuery());
                ViewBag.Categories = categories;
                return View(model);
            }
        }
        
        public async Task<IActionResult> Index()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            var mappedCourses = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<CourseDto>>(courses);

            return View(mappedCourses);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CourseDto
            {
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Dto.CourseDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courseExsist = await _unitOfWork.Repository<Course>().GetEntityAsync(model.Id);
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
                courseExsist.ImageUrl = uploadedImage.Data as string;
            }

            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Repository<Course>().GetEntityAsync(id);
            _unitOfWork.Repository<Course>().Delete(category);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}
