using AdminDashboard.Commands;
using AdminDashboard.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]    
    public class ReportController : Controller
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()  
        {
            var command = new GetAllReportsCommand();
            var response = await _mediator.Send(command);  

            if (response.StatusCode == 200)  
            {
                var reports = (IEnumerable<Dto.ReportDto>)response.Data;
                return View(reports);
            }

            return View(new List<Dto.ReportDto>());
        }
        public async Task<IActionResult> Details(int id)
        {
            var command = new GetReportDetailsCommand { Id = id };
            var response = await _mediator.Send(command);

            if (response.StatusCode == 200)
            {
                var report = (Dto.ReportDto)response.Data;
                return View(report);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> HandleReport(HandleReportDto dto)
        {
            var command = new HandleReportCommand
            {
                ReportId = dto.ReportId,
                AdminResponse = dto.AdminResponse
            };

            var response = await _mediator.Send(command);
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var command = new DeleteReportCommand { ReportId = id };
            var response = await _mediator.Send(command);
            return Json(response);
        }
    }
}
