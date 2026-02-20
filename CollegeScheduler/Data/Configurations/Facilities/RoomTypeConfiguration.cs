using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
	public void Configure(EntityTypeBuilder<RoomType> builder)
	{
		builder.HasKey(x => x.RoomTypeId);
		builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
		builder.HasIndex(x => x.Name).IsUnique();
	}
}
