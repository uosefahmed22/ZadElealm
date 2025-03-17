using AdminDashboard.Commands.QuizCommand;
using AdminDashboard.Quires.QuizQuery;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;
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
        private readonly IMediator _mediator;

        public QuizController(IUnitOfWork unitOfWork,IQuizService quizService,IMapper mapper,IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _quizService = quizService;
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var quizDtos = await _mediator.Send(new GetAllQuizzesQuery());

            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            ViewBag.Courses = courses.ToDictionary(c => c.Id, c => c.Name);

            return View(quizDtos);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var courses = await _mediator.Send(new GetCoursesForQuizQuery());
            ViewBag.Courses = courses;
            return View(new QuizDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuizDto quizDto)
        {
            if (!ModelState.IsValid)
            {
                var courses = await _mediator.Send(new GetCoursesForQuizQuery());
                ViewBag.Courses = courses;
                return View(quizDto);
            }

            var result = await _mediator.Send(new CreateQuizCommand { QuizDto = quizDto });

            if (!result)
            {
                ModelState.AddModelError("", "هذا الكورس يحتوي بالفعل على امتحان.");
                var courses = await _mediator.Send(new GetCoursesForQuizQuery());
                ViewBag.Courses = courses;
                return View(quizDto);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var spec = new QuizWithQuestionsAndChoicesSpecification(id);
            var quiz = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);

            if (quiz == null)
            {
                return NotFound();
            }

            var quizDto = _mapper.Map<QuizDto>(quiz);
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            ViewBag.Courses = courses;
            return View(quizDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(QuizDto quizDto)
        {
            if (quizDto == null)
            {
                return BadRequest("QuizDto is null");
            }

            if (!ModelState.IsValid)
            {
                var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
                ViewBag.Courses = courses;
                return View(quizDto);
            }

            try
            {
                await _quizService.UpdateQuizAsync(quizDto);
                TempData["SuccessMessage"] = "تم تحديث الامتحان بنجاح.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث الامتحان: " + ex.Message);
                var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
                ViewBag.Courses = courses;
                return View(quizDto);
            }
        }
       
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _unitOfWork.Repository<Quiz>().GetEntityAsync(id);
            _unitOfWork.Repository<Quiz>().Delete(quiz);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}

