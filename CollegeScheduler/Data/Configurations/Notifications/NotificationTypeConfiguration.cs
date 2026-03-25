using CollegeScheduler.Data.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Notifications
{
	public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
	{
		public void Configure(EntityTypeBuilder<NotificationType> b)
		{
			b.ToTable("NotificationTypes");

			b.Property(x => x.Name)
				.HasMaxLength(50)
				.IsRequired();

			b.HasIndex(x => x.Name).IsUnique();
		}
	}
}