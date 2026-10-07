using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize(Roles="Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _users; private readonly RoleManager<IdentityRole> _roles; private readonly AuditService _audit;
    public UsersController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, AuditService audit) { _users=users; _roles=roles; _audit=audit; }

    public async Task<IActionResult> Index() => View(await _users.Users.OrderBy(x=>x.Email).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string email, string fullName, string password, string role)
    {
        if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(fullName)||string.IsNullOrWhiteSpace(password)) { ModelState.AddModelError("","All fields are required."); return View(); }
        if(!await _roles.RoleExistsAsync(role)) role="SalesExecutive";
        var user=new ApplicationUser{UserName=email,Email=email,FullName=fullName,EmailConfirmed=true};
        var result=await _users.CreateAsync(user,password);
        if(result.Succeeded){ await _users.AddToRoleAsync(user,role); await _audit.LogAsync("Create","User",user.Id); return RedirectToAction(nameof(Index)); }
        foreach(var e in result.Errors) ModelState.AddModelError("",e.Description);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string id)
    {
        var user=await _users.FindByIdAsync(id); if(user==null)return NotFound();
        user.IsActive=!user.IsActive; await _users.UpdateAsync(user); await _audit.LogAsync("StatusChange","User",id,null,user.IsActive.ToString());
        return RedirectToAction(nameof(Index));
    }
}