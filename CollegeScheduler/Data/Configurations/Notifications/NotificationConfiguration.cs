using CollegeScheduler.Data.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Notifications
{
	public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
	{
		public void Configure(EntityTypeBuilder<Notification> b)
		{
			b.ToTable("Notifications");

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Message)
				.IsRequired();

			b.HasIndex(x => new { x.NotificationTypeId, x.CreatedAtUtc });

			b.HasOne(x => x.NotificationType)
				.WithMany()
				.HasForeignKey(x => x.NotificationTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.RelatedTimetableEvent)
				.WithMany()
				.HasForeignKey(x => x.RelatedTimetableEventId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.RelatedRequest)
				.WithMany()
				.HasForeignKey(x => x.RelatedRequestId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}