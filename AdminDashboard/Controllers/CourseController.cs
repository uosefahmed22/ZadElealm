using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Dto;
using AdminDashboard.Handlers.CategoryHandler;
using AdminDashboard.Models;
using AdminDashboard.Quires.CategoryQuery;
using AdminDashboard.Quires.CourseQuery;
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

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var courses = await _mediator.Send(new GetAllCoursesQuery());
            return View(courses);
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
            var query = new GetCourseByIdQuery { Id = id };
            var model = await _mediator.Send(query);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DashboardCourseDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var command = new UpdateCourseCommand
            {
                CourseDto = model
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["Success"] = "Course updated successfully";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update the course.";
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteCourseCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["Success"] = "Course deleted successfully";
            }
            else
            {
                TempData["Error"] = "Failed to delete the course";
            }

            return RedirectToAction("Index");
        }
    }
}