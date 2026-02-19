using CollegeScheduler.Data.Entities.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Profiles
{
	public class LecturerProfileConfiguration: IEntityTypeConfiguration<LecturerProfile>
	{
		public void Configure(EntityTypeBuilder<LecturerProfile> b)
		{
			b.ToTable("LecturerProfiles");

			b.HasIndex(x => x.StaffNumber).IsUnique();

			b.HasIndex(x => x.UserId)
				.IsUnique()
				.HasFilter("[UserId] IS NOT NULL");

			b.HasIndex(x => new { x.DepartmentId, x.IsActive });

			b.HasOne(x => x.User)
				.WithOne(u => u.LecturerProfile)
				.HasForeignKey<LecturerProfile>(x => x.UserId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
