using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Dto;
using AdminDashboard.Handlers.CategoryHandler;
using AdminDashboard.Models;
using AdminDashboard.Quires.CategoryQuery;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using ZadElealm.Apis.Dtos;
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
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _httpClient = httpClient;
        }
        public async Task<IActionResult> Index()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            var mappedCourses = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<DashboardCourseDto>>(courses);

            return View(mappedCourses);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _mediator.Send(new GetAllCategoriesQuery());
                ViewBag.Categories = categories;

                return View(new PlaylistViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the create course page.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlaylistViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _mediator.Send(new GetAllCategoriesQuery());
                    ViewBag.Categories = categories;
                    return View(model);
                }

                var command = new CreateCourseCommand(model);
                var response = await _mediator.Send(command);

                if (response.StatusCode == 200)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", response.Message);
                var categoriesForError = await _mediator.Send(new GetAllCategoriesQuery());
                ViewBag.Categories = categoriesForError;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while creating the course.");
                var categories = await _mediator.Send(new GetCategoriesQuery());
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new DashboardCourseDto
            {
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(DashboardCourseDto model)
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
                var uploadedImage = await _imageService.UploadImageAsync(model.formFile);
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
