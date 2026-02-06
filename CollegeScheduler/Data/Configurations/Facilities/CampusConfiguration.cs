using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class CampusConfiguration : IEntityTypeConfiguration<Campus>
{
	public void Configure(EntityTypeBuilder<Campus> builder)
	{
		builder.HasKey(x => x.CampusId);

		builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
		builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

		builder.HasIndex(x => x.Code).IsUnique();
	}
}
