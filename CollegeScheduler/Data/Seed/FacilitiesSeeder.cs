using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data.Seed;

public static class FacilitiesSeeder
{
	public static async Task SeedAsync(IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// If there is already data, don't reseed
		if (await db.Campuses.AnyAsync())
			return;

		db.Campuses.AddRange(
			new Campus { Code = "DUB", Name = "Dublin Campus", City = "Dublin", Address = "Main Street" },
			new Campus { Code = "GAL", Name = "Galway Campus", City = "Galway", Address = "Main Street" },
			new Campus { Code = "COR", Name = "Cork Campus", City = "Cork", Address = "Main Street" }
		);

		await db.SaveChangesAsync();
	}
}
