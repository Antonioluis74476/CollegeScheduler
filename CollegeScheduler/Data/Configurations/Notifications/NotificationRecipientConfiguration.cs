using CollegeScheduler.Data.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Notifications
{
	public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
	{
		public void Configure(EntityTypeBuilder<NotificationRecipient> b)
		{
			b.ToTable("NotificationRecipients");

			b.HasKey(x => new { x.NotificationId, x.UserId });

			b.Property(x => x.UserId)
				.HasMaxLength(450)
				.IsRequired();

			b.Property(x => x.DeliveryStatus)
				.HasMaxLength(20)
				.HasDefaultValue("Pending")
				.IsRequired();

			b.HasIndex(x => new { x.UserId, x.DeliveryStatus });

			b.HasOne(x => x.Notification)
				.WithMany(x => x.Recipients)
				.HasForeignKey(x => x.NotificationId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}