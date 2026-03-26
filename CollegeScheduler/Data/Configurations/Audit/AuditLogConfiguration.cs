using CollegeScheduler.Data.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Audit
{
	public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
	{
		public void Configure(EntityTypeBuilder<AuditLog> b)
		{
			b.ToTable("AuditLogs");

			b.Property(x => x.UserId)
				.HasMaxLength(450);

			b.Property(x => x.Action)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.EntityType)
				.HasMaxLength(100)
				.IsRequired();

			b.Property(x => x.EntityId)
				.HasMaxLength(100);

			b.Property(x => x.IpAddress)
				.HasMaxLength(45);

			b.Property(x => x.UserAgent)
				.HasMaxLength(300);

			b.HasIndex(x => new { x.EntityType, x.EntityId, x.PerformedAtUtc });
			b.HasIndex(x => new { x.UserId, x.PerformedAtUtc });

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}