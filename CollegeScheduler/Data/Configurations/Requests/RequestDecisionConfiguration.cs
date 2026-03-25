using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestDecisionConfiguration : IEntityTypeConfiguration<RequestDecision>
	{
		public void Configure(EntityTypeBuilder<RequestDecision> b)
		{
			b.ToTable("RequestDecisions");

			b.Property(x => x.DecidedByUserId)
				.HasMaxLength(450)
				.IsRequired();

			b.Property(x => x.Decision)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.Comment)
				.HasMaxLength(500);

			b.HasIndex(x => new { x.RequestId, x.DecidedAtUtc });

			b.HasOne(x => x.Request)
				.WithMany(x => x.Decisions)
				.HasForeignKey(x => x.RequestId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.DecidedByUser)
				.WithMany()
				.HasForeignKey(x => x.DecidedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}