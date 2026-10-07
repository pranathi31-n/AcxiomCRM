using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[ApiController, Route("api/leads"), Authorize]
public class ApiLeadsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ApiLeadsController(ApplicationDbContext db)=>_db=db;
    [HttpGet] public async Task<IActionResult> Get()=>Ok(await _db.Leads.Select(x=>new{x.LeadId,x.LeadCode,x.LeadName,x.Email,x.Phone,x.CompanyName,x.Source,x.Status,x.ExpectedValue,x.CreatedDate,x.AssignedTo}).ToListAsync());
    [HttpPost] public async Task<IActionResult> Post(Lead model){ if(!ModelState.IsValid)return ValidationProblem(ModelState); _db.Leads.Add(model); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get),new{id=model.LeadId},model); }
}