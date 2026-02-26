using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Scheduling;

public sealed class TimetableEventConfiguration : IEntityTypeConfiguration<TimetableEvent>
{
	public void Configure(EntityTypeBuilder<TimetableEvent> b)
	{
		b.ToTable("TimetableEvents");

		b.HasKey(x => x.TimetableEventId);

		b.Property(x => x.StartUtc).IsRequired().HasColumnType("datetime2");
		b.Property(x => x.EndUtc).IsRequired().HasColumnType("datetime2");

		b.Property(x => x.SessionType)
			.IsRequired()
			.HasMaxLength(20)
			.HasDefaultValue("Lecture");

		b.Property(x => x.CreatedByUserId)
			.IsRequired()
			.HasMaxLength(450);

		b.Property(x => x.CreatedAtUtc).IsRequired().HasColumnType("datetime2");
		b.Property(x => x.UpdatedAtUtc).IsRequired().HasColumnType("datetime2");

		// CHECK (EndUtc > StartUtc)
		b.ToTable(t => t.HasCheckConstraint("CK_TimetableEvents_EndAfterStart", "[EndUtc] > [StartUtc]"));

		// Relationships
		b.HasOne(x => x.Term).WithMany().HasForeignKey(x => x.TermId).OnDelete(DeleteBehavior.Restrict);
		b.HasOne(x => x.Module).WithMany().HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Restrict);
		b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
		b.HasOne(x => x.EventStatus).WithMany().HasForeignKey(x => x.EventStatusId).OnDelete(DeleteBehavior.Restrict);

		b.HasOne(x => x.CreatedByUser)
			.WithMany()
			.HasForeignKey(x => x.CreatedByUserId)
			.OnDelete(DeleteBehavior.Restrict);

		// Indexes
		b.HasIndex(x => new { x.RoomId, x.StartUtc, x.EndUtc });
		b.HasIndex(x => new { x.TermId, x.StartUtc });
		b.HasIndex(x => x.RecurrenceGroupId);
	}
}