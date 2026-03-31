using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CollegeScheduler.Data;

namespace CollegeScheduler.Data.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine(" IdentitySeeder started");
        Console.WriteLine($" Using DB: {db.Database.GetDbConnection().Database}");
        Console.WriteLine($"Connection: {db.Database.GetDbConnection().ConnectionString}");

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!createRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                    throw new Exception($" Failed to create role '{role}': {errors}");
                }

                Console.WriteLine($" Created role: {role}");
            }
            else
            {
                Console.WriteLine($" Role already exists: {role}");
            }
        }

        // Seed default Admin (dev only)
        var adminEmail = config["SeedAdmin:Email"] ?? "admin@college.ie";
        var adminPassword = config["SeedAdmin:Password"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(admin, adminPassword);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                throw new Exception($" Failed to create seed admin: {errors}");
            }

            Console.WriteLine($" Created seed admin: {adminEmail}");
        }
        else
        {
            Console.WriteLine($" Seed admin already exists: {adminEmail}");
        }

        if (!await userManager.IsInRoleAsync(admin, RoleNames.Admin))
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, RoleNames.Admin);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new Exception($" Failed to assign Admin role: {errors}");
            }

            Console.WriteLine($" Assigned Admin role to: {adminEmail}");
        }
        else
        {
            Console.WriteLine($" Admin role already assigned to: {adminEmail}");
        }

        Console.WriteLine(" IdentitySeeder finished");

    }
}
