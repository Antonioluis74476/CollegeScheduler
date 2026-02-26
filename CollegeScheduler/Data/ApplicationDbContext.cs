using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Membership;
using CollegeScheduler.Data.Entities.Scheduling;
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

		// Academic
		public DbSet<AcademicProgram> AcademicPrograms => Set<AcademicProgram>();
		public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
		public DbSet<Department> Departments => Set<Department>();
		public DbSet<Cohort> Cohorts => Set<Cohort>();
		public DbSet<Module> Modules => Set<Module>();
		public DbSet<Term> Terms => Set<Term>();
		public DbSet<CohortModule> CohortModules => Set<CohortModule>();
		public DbSet<ModuleLecturer> ModuleLecturers => Set<ModuleLecturer>();

		//Membership
		public DbSet<StudentCohortMembership> StudentCohortMemberships => Set<StudentCohortMembership>();
		public DbSet<StudentModuleEnrollment> StudentModuleEnrollments => Set<StudentModuleEnrollment>();

		// Scheduling
		public DbSet<EventStatus> EventStatuses => Set<EventStatus>();
		public DbSet<TimetableEvent> TimetableEvents => Set<TimetableEvent>();
		public DbSet<EventCohort> EventCohorts => Set<EventCohort>();
		public DbSet<EventLecturer> EventLecturers => Set<EventLecturer>();
		public DbSet<TimetableEventChange> TimetableEventChanges => Set<TimetableEventChange>();

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
