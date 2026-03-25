using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Scheduling;

public sealed class EventStatusConfiguration : IEntityTypeConfiguration<EventStatus>
{
	public void Configure(EntityTypeBuilder<EventStatus> b)
	{
		b.ToTable("EventStatuses");

		b.HasKey(x => x.EventStatusId);

		b.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(20);

		b.HasIndex(x => x.Name).IsUnique();

		// Optional seed (helps testing immediately)
		b.HasData(
			new EventStatus { EventStatusId = 1, Name = "Scheduled" },
			new EventStatus { EventStatusId = 2, Name = "Cancelled" },
			new EventStatus { EventStatusId = 3, Name = "Moved" },
			new EventStatus { EventStatusId = 4, Name = "Completed" }
		);
	}
}