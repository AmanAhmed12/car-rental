using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;

namespace carRentalProject.Controllers;

public class GuestDashboardController : Controller
{
    private readonly ILogger<GuestDashboardController> _logger;

    public GuestDashboardController(ILogger<GuestDashboardController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
