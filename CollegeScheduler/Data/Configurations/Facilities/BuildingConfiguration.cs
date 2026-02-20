using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
	public void Configure(EntityTypeBuilder<Building> builder)
	{
		builder.HasKey(x => x.BuildingId);

		builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
		builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

		builder.HasIndex(x => new { x.CampusId, x.Code }).IsUnique();

		builder.HasOne(x => x.Campus)
			.WithMany(c => c.Buildings)
			.HasForeignKey(x => x.CampusId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
