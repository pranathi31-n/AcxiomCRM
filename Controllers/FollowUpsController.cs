using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _db; private readonly AuditService _audit;
    public FollowUpsController(ApplicationDbContext db, AuditService audit) { _db=db; _audit=audit; }

    public async Task<IActionResult> Index()
    {
        var q=_db.FollowUps.AsQueryable();
        if(User.IsInRole("SalesExecutive")) q=q.Where(x=>x.AssignedTo==User.FindFirstValue(ClaimTypes.NameIdentifier));
        return View(await q.OrderBy(x=>x.FollowUpDate).ToListAsync());
    }

    public IActionResult Create() => View(new FollowUp { FollowUpDate=DateTime.Today });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp model)
    {
        if(!ModelState.IsValid) return View(model);
        model.AssignedTo ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
        _db.FollowUps.Add(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Create","FollowUp",model.FollowUpId.ToString());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var f=await _db.FollowUps.FindAsync(id); if(f==null) return NotFound();
        f.Status="Completed"; await _db.SaveChangesAsync(); await _audit.LogAsync("Complete","FollowUp",id.ToString());
        return RedirectToAction(nameof(Index));
    }
}