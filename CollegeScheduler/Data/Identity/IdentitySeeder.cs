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

        // Seed default Lecturer test account
        var lecturerEmail = "lecturertest@college.ie";
        var lecturerPassword = "Lecturer123!";

        var lecturerUser = await userManager.FindByEmailAsync(lecturerEmail);

        if (lecturerUser is null)
        {
            lecturerUser = new ApplicationUser
            {
                UserName = lecturerEmail,
                Email = lecturerEmail,
                EmailConfirmed = true
            };

            var lecturerResult =
                await userManager.CreateAsync(lecturerUser, lecturerPassword);

            if (!lecturerResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    lecturerResult.Errors.Select(e => e.Description));

                throw new Exception(
                    $"Failed to create seed lecturer: {errors}");
            }

            Console.WriteLine($"Created seed lecturer: {lecturerEmail}");
        }

        if (!await userManager.IsInRoleAsync(lecturerUser, RoleNames.Lecturer))
        {
            var lecturerRoleResult =
                await userManager.AddToRoleAsync(
                    lecturerUser,
                    RoleNames.Lecturer);

            if (!lecturerRoleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    lecturerRoleResult.Errors.Select(e => e.Description));

                throw new Exception(
                    $"Failed to assign Lecturer role: {errors}");
            }

            Console.WriteLine(
                $"Assigned Lecturer role to: {lecturerEmail}");
        }

        // Seed default Student test account
        var studentEmail = "studenttest@college.ie";
        var studentPassword = "Student123!";

        var studentUser = await userManager.FindByEmailAsync(studentEmail);

        if (studentUser is null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                EmailConfirmed = true
            };

            var studentResult =
                await userManager.CreateAsync(studentUser, studentPassword);

            if (!studentResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    studentResult.Errors.Select(e => e.Description));

                throw new Exception(
                    $"Failed to create seed student: {errors}");
            }

            Console.WriteLine($"Created seed student: {studentEmail}");
        }

        if (!await userManager.IsInRoleAsync(studentUser, RoleNames.Student))
        {
            var studentRoleResult =
                await userManager.AddToRoleAsync(
                    studentUser,
                    RoleNames.Student);

            if (!studentRoleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    studentRoleResult.Errors.Select(e => e.Description));

                throw new Exception(
                    $"Failed to assign Student role: {errors}");
            }

            Console.WriteLine(
                $"Assigned Student role to: {studentEmail}");
        }

        Console.WriteLine(" IdentitySeeder finished");

    }
}
