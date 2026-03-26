using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Scheduling;

public sealed class EventCohortConfiguration : IEntityTypeConfiguration<EventCohort>
{
	public void Configure(EntityTypeBuilder<EventCohort> b)
	{
		b.ToTable("EventCohorts");

		b.HasKey(x => new { x.TimetableEventId, x.CohortId });

		b.HasOne(x => x.TimetableEvent)
			.WithMany(e => e.EventCohorts)
			.HasForeignKey(x => x.TimetableEventId)
			.OnDelete(DeleteBehavior.Cascade);

		b.HasOne(x => x.Cohort)
			.WithMany()
			.HasForeignKey(x => x.CohortId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasIndex(x => x.CohortId);
	}
}