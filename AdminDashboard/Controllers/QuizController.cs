﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Service.AppServices;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles ="Admin")]
    public class QuizController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuizService _quizService;
        private readonly IMapper _mapper;

        public QuizController(IUnitOfWork unitOfWork,IQuizService quizService,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _quizService = quizService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var quizzes = await _unitOfWork.Repository<Quiz>().GetAllAsync();
            var questions = await _unitOfWork.Repository<Question>().GetAllAsync();

            var quizDtos = quizzes.Select(q => new QuizDto
            {
                Id = q.Id,
                Name = q.Name,
                Description = q.Description,
                CourseId = q.CourseId,
                QuestionCount = questions.Count(question => question.QuizId == q.Id) 
            }).ToList();

            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            ViewBag.Courses = courses.ToDictionary(c => c.Id, c => c.Name);

            return View(quizDtos);
        }
        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            ViewBag.Courses = courses;
            return View(new QuizDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuizDto quizDto)
        {
            if (!ModelState.IsValid)
            {
                var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
                ViewBag.Courses = courses;
                return View(quizDto);
            }

            var spec = new QuizWithCourseSpecification(quizDto.CourseId);
            var existingQuiz = await _unitOfWork.Repository<Quiz>()
                .GetEntityWithSpecAsync(spec);

            if (existingQuiz != null)
            {
                ModelState.AddModelError("", "هذا الكورس يحتوي بالفعل على امتحان.");
                var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
                ViewBag.Courses = courses;
                return View(quizDto);
            }

            await _quizService.CreateQuizAsync(quizDto);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _unitOfWork.Repository<Quiz>().GetByIdAsync(id);
            _unitOfWork.Repository<Quiz>().Delete(quiz);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
       
    }
}

