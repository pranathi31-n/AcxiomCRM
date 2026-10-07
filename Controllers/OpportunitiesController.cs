using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class OpportunitiesController : Controller
{
    private readonly ApplicationDbContext _db; private readonly AuditService _audit;
    public OpportunitiesController(ApplicationDbContext db, AuditService audit) { _db=db; _audit=audit; }

    public async Task<IActionResult> Index(string? search, string? stage)
    {
        var q=_db.Opportunities.Include(x=>x.Customer).AsQueryable();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.OpportunityName.Contains(search));
        if(!string.IsNullOrWhiteSpace(stage)) q=q.Where(x=>x.Stage==stage);
        if(User.IsInRole("SalesExecutive")) q=q.Where(x=>x.AssignedTo==User.FindFirstValue(ClaimTypes.NameIdentifier));
        return View(await q.OrderByDescending(x=>x.CreatedDate).ToListAsync());
    }

    public async Task<IActionResult> Create() => View(new Opportunity { CustomerId=(await _db.Customers.Select(x=>(int?)x.CustomerId).FirstOrDefaultAsync()) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Opportunity model)
    {
        if(!ModelState.IsValid) return View(model);
        model.AssignedTo ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
        _db.Opportunities.Add(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Create","Opportunity",model.OpportunityId.ToString());
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _db.Opportunities.FindAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Opportunity model)
    {
        if(id!=model.OpportunityId) return NotFound();
        if(!ModelState.IsValid) return View(model);
        _db.Update(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Update","Opportunity",id.ToString());
        return RedirectToAction(nameof(Index));
    }
}