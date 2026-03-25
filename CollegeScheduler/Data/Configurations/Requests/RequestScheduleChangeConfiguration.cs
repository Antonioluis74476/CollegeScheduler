using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestScheduleChangeConfiguration : IEntityTypeConfiguration<RequestScheduleChange>
	{
		public void Configure(EntityTypeBuilder<RequestScheduleChange> b)
		{
			b.ToTable("RequestScheduleChanges");

			b.HasKey(x => x.RequestId);

			b.Property(x => x.Reason)
				.HasMaxLength(800)
				.IsRequired();

			b.HasIndex(x => x.TimetableEventId);

			b.ToTable(t => t.HasCheckConstraint(
				"CK_RequestScheduleChange_ProposedEndUtc_ProposedStartUtc",
				"[ProposedEndUtc] IS NULL OR [ProposedStartUtc] IS NULL OR [ProposedEndUtc] > [ProposedStartUtc]"));

			b.HasOne(x => x.Request)
				.WithOne(x => x.ScheduleChangeDetail)
				.HasForeignKey<RequestScheduleChange>(x => x.RequestId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TimetableEvent)
				.WithMany()
				.HasForeignKey(x => x.TimetableEventId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProposedRoom)
				.WithMany()
				.HasForeignKey(x => x.ProposedRoomId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}