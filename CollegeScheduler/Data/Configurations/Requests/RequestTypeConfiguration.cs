using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
	{
		public void Configure(EntityTypeBuilder<RequestType> b)
		{
			b.ToTable("RequestTypes");

			b.Property(x => x.Name)
				.HasMaxLength(30)
				.IsRequired();

			b.HasIndex(x => x.Name).IsUnique();
		}
	}
}