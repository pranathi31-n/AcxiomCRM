using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "Manager", "SalesExecutive" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        await CreateUser(userManager, "admin@acxiomcrm.local", "Admin User", "Admin123!");
        await CreateUser(userManager, "manager@acxiomcrm.local", "Manager User", "Manager123!");
        await CreateUser(userManager, "sales@acxiomcrm.local", "Sales Executive", "Sales123!");

        if (!db.Customers.Any())
        {
            var admin = await userManager.FindByEmailAsync("admin@acxiomcrm.local");
            var manager = await userManager.FindByEmailAsync("manager@acxiomcrm.local");
            db.Customers.AddRange(
                new Customer { CustomerCode="CUS-001", CustomerName="Acme Industries", Email="contact@acme.example", Phone="9876543210", CompanyName="Acme Industries", City="Vijayawada", State="Andhra Pradesh", CreatedBy=admin?.Id },
                new Customer { CustomerCode="CUS-002", CustomerName="Bright Solutions", Email="hello@bright.example", Phone="9123456789", CompanyName="Bright Solutions", City="Hyderabad", State="Telangana", CreatedBy=manager?.Id });
            await db.SaveChangesAsync();
        }
    }

    private static async Task CreateUser(UserManager<ApplicationUser> manager, string email, string name, string password)
    {
        if (await manager.FindByEmailAsync(email) != null) return;
        var user = new ApplicationUser { UserName=email, Email=email, FullName=name, EmailConfirmed=true, IsActive=true };
        var result = await manager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            var role = email.StartsWith("admin") ? "Admin" : email.StartsWith("manager") ? "Manager" : "SalesExecutive";
            await manager.AddToRoleAsync(user, role);
        }
    }
}