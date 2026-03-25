using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Scheduling;

public sealed class TimetableEventChangeConfiguration : IEntityTypeConfiguration<TimetableEventChange>
{
	public void Configure(EntityTypeBuilder<TimetableEventChange> b)
	{
		b.ToTable("TimetableEventChanges");

		b.HasKey(x => x.TimetableEventChangeId);

		b.Property(x => x.ChangeType).IsRequired().HasMaxLength(20);
		b.Property(x => x.Reason).IsRequired().HasMaxLength(500);

		b.Property(x => x.ChangedByUserId).IsRequired().HasMaxLength(450);
		b.Property(x => x.ChangedAtUtc).IsRequired().HasColumnType("datetime2");

		b.Property(x => x.NotificationSent).HasDefaultValue(false);

		b.HasOne(x => x.TimetableEvent)
			.WithMany(e => e.Changes)
			.HasForeignKey(x => x.TimetableEventId)
			.OnDelete(DeleteBehavior.Cascade);

		b.HasOne(x => x.ChangedByUser)
			.WithMany()
			.HasForeignKey(x => x.ChangedByUserId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasOne(x => x.OldRoom)
			.WithMany()
			.HasForeignKey(x => x.OldRoomId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasOne(x => x.NewRoom)
			.WithMany()
			.HasForeignKey(x => x.NewRoomId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasIndex(x => new { x.TimetableEventId, x.ChangedAtUtc });
		b.HasIndex(x => x.NotificationSent);
	}
}