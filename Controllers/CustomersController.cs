using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public CustomersController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var q = _db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            q = q.Where(x =>
                x.CustomerName.Contains(search) ||
                x.Email.Contains(search) ||
                x.Phone.Contains(search) ||
                x.CompanyName.Contains(search));
        }

        if (User.IsInRole("SalesExecutive"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            q = q.Where(x => x.CreatedBy == userId);
        }

        return View(await q
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers
            .Include(x => x.Opportunities)
            .FirstOrDefaultAsync(x => x.CustomerId == id);

        if (customer == null)
            return NotFound();

        return View(customer);
    }

    public IActionResult Create()
    {
        return View(new Customer
        {
            CustomerCode = "CUS-" + DateTime.UtcNow.Ticks.ToString()[^6..]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer model)
    {
        // Check duplicate email
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            if (await _db.Customers.AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email already exists.");
            }
        }

        // Check duplicate phone
        if (!string.IsNullOrWhiteSpace(model.Phone))
        {
            if (await _db.Customers.AnyAsync(x => x.Phone == model.Phone))
            {
                ModelState.AddModelError(
                    nameof(model.Phone),
                    "Phone already exists.");
            }
        }

        // If validation fails, return the form
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e =>
                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? $"{x.Key}: Invalid value."
                        : $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            TempData["CreateErrors"] = string.Join(" | ", errors);

            return View(model);
        }

        // Set the logged-in user as creator
        model.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Save customer
        _db.Customers.Add(model);
        await _db.SaveChangesAsync();

        // Audit log
        await _audit.LogAsync(
            "Create",
            "Customer",
            model.CustomerId.ToString(),
            null,
            model.CustomerName);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _db.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer model)
    {
        if (id != model.CustomerId)
            return NotFound();

        // Check whether another customer already uses this email or phone
        if (await _db.Customers.AnyAsync(x =>
            x.CustomerId != id &&
            (x.Email == model.Email || x.Phone == model.Phone)))
        {
            ModelState.AddModelError(
                "",
                "Email or phone already belongs to another customer.");
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.Update(model);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            "Update",
            "Customer",
            id.ToString());

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        customer.Status = "Inactive";

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            "Delete/Deactivate",
            "Customer",
            id.ToString());

        return RedirectToAction(nameof(Index));
    }
}