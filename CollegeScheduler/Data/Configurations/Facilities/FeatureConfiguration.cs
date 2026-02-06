using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
	public void Configure(EntityTypeBuilder<Feature> builder)
	{
		builder.HasKey(x => x.FeatureId);
		builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
		builder.HasIndex(x => x.Name).IsUnique();
	}
}
