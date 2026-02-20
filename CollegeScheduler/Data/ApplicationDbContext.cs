using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Profiles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options) { }

		// Facilities
		public DbSet<Campus> Campuses => Set<Campus>();
		public DbSet<Building> Buildings => Set<Building>();
		public DbSet<RoomType> RoomTypes => Set<RoomType>();
		public DbSet<Room> Rooms => Set<Room>();
		public DbSet<Feature> Features => Set<Feature>();
		public DbSet<RoomFeature> RoomFeatures => Set<RoomFeature>();
		public DbSet<UnavailabilityReasonType> UnavailabilityReasonTypes => Set<UnavailabilityReasonType>();
		public DbSet<RoomUnavailability> RoomUnavailabilities => Set<RoomUnavailability>();

		// Profiles
		public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
		public DbSet<LecturerProfile> LecturerProfiles => Set<LecturerProfile>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Automatically applies all IEntityTypeConfiguration<T> from this assembly
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
					entry.Entity.CreatedAtUtc = now;
					entry.Entity.UpdatedAtUtc = null;
				}
				else if (entry.State == EntityState.Modified)
				{
					// Don’t allow CreatedAt to be changed by accident
					entry.Property(x => x.CreatedAtUtc).IsModified = false;

					// Optional: only stamp UpdatedAt if something actually changed
					if (entry.Properties.Any(p => p.IsModified))
					{
						entry.Entity.UpdatedAtUtc = now;
					}
				}
			}
		}
	}
}
