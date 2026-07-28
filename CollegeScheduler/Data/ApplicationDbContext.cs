using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Membership;
using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Data.Entities.Audit;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace CollegeScheduler.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options) { }

        // Flag to prevent recursive audit logging when saving audit logs themselves
        private bool _savingAuditLogs;

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

		//Requests
		public DbSet<RequestType> RequestTypes => Set<RequestType>();
		public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
		public DbSet<Request> Requests => Set<Request>();
		public DbSet<RequestRoomBooking> RequestRoomBookings => Set<RequestRoomBooking>();
		public DbSet<RequestScheduleChange> RequestScheduleChanges => Set<RequestScheduleChange>();
		public DbSet<RequestDecision> RequestDecisions => Set<RequestDecision>();

		// Notifications
		public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
		public DbSet<Notification> Notifications => Set<Notification>();
		public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

		// Audit
		public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Automatically applies all IEntityTypeConfiguration<T> from this assembly
			builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}

        public override int SaveChanges()
        {
            ApplyAuditInfo();

            if (_savingAuditLogs)
                return base.SaveChanges();

            var pendingAudits = CaptureAuditEntries();

            var result = base.SaveChanges();

            SaveAuditEntries(pendingAudits);

            return result;
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();

            if (_savingAuditLogs)
                return await base.SaveChangesAsync(cancellationToken);

            var pendingAudits = CaptureAuditEntries();

            var result = await base.SaveChangesAsync(cancellationToken);

            await SaveAuditEntriesAsync(pendingAudits, cancellationToken);

            return result;
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

        private List<PendingAuditEntry> CaptureAuditEntries()
        {
            ChangeTracker.DetectChanges();

            var audits = new List<PendingAuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is not AuditableEntity)
                    continue;

                if (entry.Entity is AuditLog)
                    continue;

                if (entry.State is not (
                    EntityState.Added or
                    EntityState.Modified or
                    EntityState.Deleted))
                {
                    continue;
                }

                var audit = new PendingAuditEntry
                {
                    Entry = entry,
                    Action = entry.State switch
                    {
                        EntityState.Added => "Create",
                        EntityState.Modified => "Update",
                        EntityState.Deleted => "Delete",
                        _ => string.Empty
                    },
                    EntityType = entry.Metadata.ClrType.Name
                };

                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    audit.EntityId = GetPrimaryKey(entry);

                    audit.OldValues = entry.Properties.ToDictionary(
                        property => property.Metadata.Name,
                        property => property.OriginalValue);
                }

                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    audit.NewValues = entry.Properties.ToDictionary(
                        property => property.Metadata.Name,
                        property => property.CurrentValue);
                }

                audits.Add(audit);
            }

            return audits;
        }

        private void SaveAuditEntries(List<PendingAuditEntry> pendingAudits)
        {
            if (pendingAudits.Count == 0)
                return;

            AddAuditLogs(pendingAudits);

            _savingAuditLogs = true;

            try
            {
                ApplyAuditInfo();
                base.SaveChanges();
            }
            finally
            {
                _savingAuditLogs = false;
            }
        }

        private async Task SaveAuditEntriesAsync(
            List<PendingAuditEntry> pendingAudits,
            CancellationToken cancellationToken)
        {
            if (pendingAudits.Count == 0)
                return;

            AddAuditLogs(pendingAudits);

            _savingAuditLogs = true;

            try
            {
                ApplyAuditInfo();
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _savingAuditLogs = false;
            }
        }

        private void AddAuditLogs(IEnumerable<PendingAuditEntry> pendingAudits)
        {
            foreach (var pending in pendingAudits)
            {
                if (pending.Action == "Create")
                {
                    pending.EntityId = GetPrimaryKey(pending.Entry);

                    pending.NewValues = pending.Entry.Properties.ToDictionary(
                        property => property.Metadata.Name,
                        property => property.CurrentValue);
                }

                AuditLogs.Add(new AuditLog
                {
                    Action = pending.Action,
                    EntityType = pending.EntityType,
                    EntityId = pending.EntityId,
                    OldValuesJson = pending.OldValues.Count == 0
                        ? null
                        : JsonSerializer.Serialize(pending.OldValues),
                    NewValuesJson = pending.NewValues.Count == 0
                        ? null
                        : JsonSerializer.Serialize(pending.NewValues),
                    PerformedAtUtc = DateTime.UtcNow,
                    UserId = null,
                    IpAddress = null,
                    UserAgent = null
                });
            }
        }

        private static string? GetPrimaryKey(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();

            if (primaryKey is null)
                return null;

            var values = primaryKey.Properties
                .Select(property =>
                    entry.Property(property.Name).CurrentValue?.ToString())
                .Where(value => !string.IsNullOrWhiteSpace(value));

            var result = string.Join(",", values);

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        private sealed class PendingAuditEntry
        {
            public required EntityEntry Entry { get; init; }

            public string Action { get; init; } = string.Empty;

            public string EntityType { get; init; } = string.Empty;

            public string? EntityId { get; set; }

            public Dictionary<string, object?> OldValues { get; set; } = [];

            public Dictionary<string, object?> NewValues { get; set; } = [];
        }


    }
}
