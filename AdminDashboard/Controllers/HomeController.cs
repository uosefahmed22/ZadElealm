using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using AdminDashboard.Queries.HomeQuery;
using MediatR;

namespace AdminDashboard.Controllers;
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMediator _mediator;

    public HomeController(ILogger<HomeController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var overview = await _mediator.Send(new GetDashboardOverviewQuery(), cancellationToken);
        return View(overview);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
