using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options) { }

		public DbSet<Campus> Campuses => Set<Campus>();
		public DbSet<Building> Buildings => Set<Building>();
		public DbSet<RoomType> RoomTypes => Set<RoomType>();
		public DbSet<Room> Rooms => Set<Room>();
		public DbSet<Feature> Features => Set<Feature>();
		public DbSet<RoomFeature> RoomFeatures => Set<RoomFeature>();
		public DbSet<UnavailabilityReasonType> UnavailabilityReasonTypes => Set<UnavailabilityReasonType>();
		public DbSet<RoomUnavailability> RoomUnavailabilities => Set<RoomUnavailability>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// This will automatically apply all IEntityTypeConfiguration<T>
			// classes you create (CampusConfiguration, RoomConfiguration, etc.)
			builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}
		public override int SaveChanges()
		{
			ApplyAuditInfo();
			return base.SaveChanges();
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ApplyAuditInfo();
			return base.SaveChangesAsync(cancellationToken);
		}

		private void ApplyAuditInfo()
		{
			var now = DateTime.UtcNow;

			foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
			{
				if (entry.State == EntityState.Added)
				{
					// If you always want DB-context to control this, overwrite it:
					entry.Entity.CreatedAtUtc = now;

					// New rows have not been updated yet
					entry.Entity.UpdatedAtUtc = null;
				}
				else if (entry.State == EntityState.Modified)
				{
					// Don’t allow CreatedAt to be changed by accident
					entry.Property(x => x.CreatedAtUtc).IsModified = false;

					entry.Entity.UpdatedAtUtc = now;
				}
			}
		}
	}
}
