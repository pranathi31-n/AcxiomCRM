using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _db; private readonly AuditService _audit;
    public LeadsController(ApplicationDbContext db, AuditService audit) { _db=db; _audit=audit; }

    public async Task<IActionResult> Index(string? search, string? status)
    {
        var q=_db.Leads.AsQueryable();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.LeadName.Contains(search)||x.CompanyName.Contains(search));
        if(!string.IsNullOrWhiteSpace(status)) q=q.Where(x=>x.Status==status);
        if(User.IsInRole("SalesExecutive")) q=q.Where(x=>x.AssignedTo==User.FindFirstValue(ClaimTypes.NameIdentifier));
        return View(await q.OrderByDescending(x=>x.CreatedDate).ToListAsync());
    }

    public IActionResult Create() => View(new Lead { LeadCode="LEAD-"+DateTime.UtcNow.Ticks.ToString()[^6..] });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lead model)
    {
        if(!ModelState.IsValid) return View(model);
        model.AssignedTo ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
        _db.Leads.Add(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Create","Lead",model.LeadId.ToString(),null,model.LeadName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _db.Leads.FindAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lead model)
    {
        if(id!=model.LeadId) return NotFound();
        if(!ModelState.IsValid) return View(model);
        _db.Update(model); await _db.SaveChangesAsync(); await _audit.LogAsync("Update","Lead",id.ToString());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Convert(int id)
    {
        var lead=await _db.Leads.FindAsync(id); if(lead==null) return NotFound();
        if(lead.Status!="Qualified") { TempData["Error"]="Only Qualified leads can be converted."; return RedirectToAction(nameof(Index)); }
        lead.Status="Converted"; await _db.SaveChangesAsync();
        await _audit.LogAsync("Convert","Lead",id.ToString(), "Qualified", "Converted");
        return RedirectToAction(nameof(Index));
    }
}