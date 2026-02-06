using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class RoomFeatureConfiguration : IEntityTypeConfiguration<RoomFeature>
{
	public void Configure(EntityTypeBuilder<RoomFeature> builder)
	{
		builder.HasKey(x => new { x.RoomId, x.FeatureId });

		builder.HasOne(x => x.Room)
			.WithMany(r => r.RoomFeatures)
			.HasForeignKey(x => x.RoomId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Feature)
			.WithMany(f => f.RoomFeatures)
			.HasForeignKey(x => x.FeatureId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
