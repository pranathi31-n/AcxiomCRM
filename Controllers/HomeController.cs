using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly DashboardService _dashboard;
    public HomeController(DashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var scope = User.IsInRole("Admin") || User.IsInRole("Manager");
        return View(await _dashboard.GetAsync(uid, scope));
    }
}