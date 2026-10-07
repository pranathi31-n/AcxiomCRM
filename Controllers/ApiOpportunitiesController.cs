using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[ApiController, Route("api/opportunities"), Authorize]
public class ApiOpportunitiesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ApiOpportunitiesController(ApplicationDbContext db)=>_db=db;
    [HttpGet] public async Task<IActionResult> Get()=>Ok(await _db.Opportunities.Select(x=>new{x.OpportunityId,x.OpportunityName,x.CustomerId,x.Amount,x.Stage,x.Probability,x.ExpectedCloseDate,x.Status,x.AssignedTo}).ToListAsync());
    [HttpPost] public async Task<IActionResult> Post(Opportunity model){ if(!ModelState.IsValid)return ValidationProblem(ModelState); _db.Opportunities.Add(model); await _db.SaveChangesAsync(); return Created("",new{model.OpportunityId,model.OpportunityName,model.Amount,model.Stage,model.Probability}); }
}