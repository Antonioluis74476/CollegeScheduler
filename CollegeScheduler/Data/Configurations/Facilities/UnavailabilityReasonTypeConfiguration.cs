using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Facilities;

public class UnavailabilityReasonTypeConfiguration : IEntityTypeConfiguration<UnavailabilityReasonType>
{
	public void Configure(EntityTypeBuilder<UnavailabilityReasonType> builder)
	{
		builder.HasKey(x => x.UnavailabilityReasonTypeId);
		builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
		builder.HasIndex(x => x.Name).IsUnique();
	}
}
