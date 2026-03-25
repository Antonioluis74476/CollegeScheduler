using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestConfiguration : IEntityTypeConfiguration<Request>
	{
		public void Configure(EntityTypeBuilder<Request> b)
		{
			b.ToTable("Requests");

			b.Property(x => x.Title)
				.HasMaxLength(200);

			b.Property(x => x.RequestedByUserId)
				.HasMaxLength(450)
				.IsRequired();

			b.HasIndex(x => new { x.RequestStatusId, x.CreatedAtUtc });
			b.HasIndex(x => new { x.RequestedByUserId, x.RequestStatusId });

			b.HasOne(x => x.RequestType)
				.WithMany()
				.HasForeignKey(x => x.RequestTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.RequestStatus)
				.WithMany()
				.HasForeignKey(x => x.RequestStatusId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.RequestedByUser)
				.WithMany()
				.HasForeignKey(x => x.RequestedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}