using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestStatusConfiguration : IEntityTypeConfiguration<RequestStatus>
	{
		public void Configure(EntityTypeBuilder<RequestStatus> b)
		{
			b.ToTable("RequestStatuses");

			b.Property(x => x.Name)
				.HasMaxLength(20)
				.IsRequired();

			b.HasIndex(x => x.Name).IsUnique();
		}
	}
}