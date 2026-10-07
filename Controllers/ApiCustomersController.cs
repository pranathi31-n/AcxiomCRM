using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class ApiCustomersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ApiCustomersController(ApplicationDbContext db)=>_db=db;

    [HttpGet] public async Task<IActionResult> Get() =>
        Ok(await _db.Customers.Select(x=>new { x.CustomerId,x.CustomerCode,x.CustomerName,x.Email,x.Phone,x.CompanyName,x.Status,x.CreatedDate }).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var x=await _db.Customers.FindAsync(id);
        return x==null?NotFound():Ok(new {x.CustomerId,x.CustomerCode,x.CustomerName,x.Email,x.Phone,x.CompanyName,x.Status,x.CreatedDate});
    }

    [HttpPost]
    public async Task<IActionResult> Post(Customer model)
    {
        if(!ModelState.IsValid) return ValidationProblem(ModelState);
        if(await _db.Customers.AnyAsync(x=>x.Email==model.Email||x.Phone==model.Phone)) return Conflict(new {message="Email or phone already exists."});
        _db.Customers.Add(model); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get),new{id=model.CustomerId},new{model.CustomerId,model.CustomerCode,model.CustomerName,model.Email,model.Phone,model.Status});
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Customer model)
    {
        if(id!=model.CustomerId)return BadRequest();
        if(!ModelState.IsValid)return ValidationProblem(ModelState);
        _db.Entry(model).State=EntityState.Modified; await _db.SaveChangesAsync(); return Ok(new{message="Updated"});
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles="Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var x=await _db.Customers.FindAsync(id); if(x==null)return NotFound();
        x.Status="Inactive"; await _db.SaveChangesAsync(); return NoContent();
    }
}