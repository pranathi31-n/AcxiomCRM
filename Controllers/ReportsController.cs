using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Customers = await _db.Customers.CountAsync();

        ViewBag.Leads = await _db.Leads.CountAsync();

        ViewBag.Won = await _db.Opportunities
            .CountAsync(x => x.Status == "Won");

        ViewBag.Lost = await _db.Opportunities
            .CountAsync(x => x.Status == "Lost");

        // SQLite does not support SumAsync on decimal values.
        // Get the amounts first, then calculate the sum in .NET.
        var pipelineAmounts = await _db.Opportunities
            .Where(x => x.Status == "Open")
            .Select(x => x.Amount)
            .ToListAsync();

        ViewBag.Pipeline = pipelineAmounts.Sum();

        ViewBag.Overdue = await _db.FollowUps
            .CountAsync(x =>
                x.Status == "Planned" &&
                x.FollowUpDate < DateTime.Today);

        return View();
    }
}