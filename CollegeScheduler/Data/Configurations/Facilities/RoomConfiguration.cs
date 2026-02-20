using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
	public void Configure(EntityTypeBuilder<Room> builder)
	{
		builder.HasKey(x => x.RoomId);

		builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
		builder.HasIndex(x => new { x.BuildingId, x.Code }).IsUnique();

		builder.HasOne(x => x.Building)
			.WithMany(b => b.Rooms)
			.HasForeignKey(x => x.BuildingId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.RoomType)
			.WithMany(rt => rt.Rooms)
			.HasForeignKey(x => x.RoomTypeId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
