using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class ActivitiesController : Controller
{
    private readonly ApplicationDbContext _db; private readonly AuditService _audit;
    public ActivitiesController(ApplicationDbContext db, AuditService audit) { _db=db; _audit=audit; }

    public async Task<IActionResult> Index() => View(await _db.Activities.OrderByDescending(x=>x.ActivityDate).ToListAsync());

    public IActionResult Create() => View(new Activity { ActivityDate=DateTime.Now });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Activity model)
    {
        if(!ModelState.IsValid) return View(model);
        model.AssignedTo ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
        _db.Activities.Add(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Create","Activity",model.ActivityId.ToString());
        return RedirectToAction(nameof(Index));
    }
}