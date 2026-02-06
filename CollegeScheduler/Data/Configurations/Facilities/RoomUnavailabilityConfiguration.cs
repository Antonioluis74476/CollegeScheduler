using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class RoomUnavailabilityConfiguration : IEntityTypeConfiguration<RoomUnavailability>
{
	public void Configure(EntityTypeBuilder<RoomUnavailability> builder)
	{
		builder.HasKey(x => x.RoomUnavailabilityId);

		builder.HasOne(x => x.Room)
			.WithMany(r => r.Unavailabilities)
			.HasForeignKey(x => x.RoomId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.UnavailabilityReasonType)
			.WithMany(t => t.RoomUnavailabilities)
			.HasForeignKey(x => x.UnavailabilityReasonTypeId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.CreatedByUser)
			.WithMany()
			.HasForeignKey(x => x.CreatedByUserId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.ToTable(t =>
		{
			t.HasCheckConstraint("CK_RoomUnavailability_EndAfterStart", "[EndUtc] > [StartUtc]");
		});
	}
}
