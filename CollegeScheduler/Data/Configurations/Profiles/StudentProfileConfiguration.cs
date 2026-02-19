using CollegeScheduler.Data.Entities.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
	public void Configure(EntityTypeBuilder<StudentProfile> b)
	{
		b.ToTable("StudentProfiles");

		b.HasIndex(x => x.StudentNumber).IsUnique();
		b.HasIndex(x => x.Status);

		b.HasIndex(x => x.UserId)
			.IsUnique()
			.HasFilter("[UserId] IS NOT NULL");

		b.HasOne(x => x.User)
			.WithOne(u => u.StudentProfile)
			.HasForeignKey<StudentProfile>(x => x.UserId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
