using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Scheduling;

public sealed class EventLecturerConfiguration : IEntityTypeConfiguration<EventLecturer>
{
	public void Configure(EntityTypeBuilder<EventLecturer> b)
	{
		b.ToTable("EventLecturers");

		b.HasKey(x => new { x.TimetableEventId, x.LecturerId });

		b.HasOne(x => x.TimetableEvent)
			.WithMany(e => e.EventLecturers)
			.HasForeignKey(x => x.TimetableEventId)
			.OnDelete(DeleteBehavior.Cascade);

		b.HasOne(x => x.Lecturer)
			.WithMany()
			.HasForeignKey(x => x.LecturerId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasIndex(x => x.LecturerId);
	}
}