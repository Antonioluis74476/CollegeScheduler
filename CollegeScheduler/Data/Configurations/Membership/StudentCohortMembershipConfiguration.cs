using CollegeScheduler.Data.Entities.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Membership
{
	public sealed class StudentCohortMembershipConfiguration : IEntityTypeConfiguration<StudentCohortMembership>
	{
		public void Configure(EntityTypeBuilder<StudentCohortMembership> b)
		{
			b.ToTable("StudentCohortMemberships");

			b.HasKey(x => new { x.StudentId, x.CohortId, x.AcademicYearId });

			b.Property(x => x.MembershipType)
				.IsRequired()
				.HasMaxLength(20);

			b.Property(x => x.StartDate).HasColumnType("date");
			b.Property(x => x.EndDate).HasColumnType("date");

			b.HasOne(x => x.Student)
				.WithMany()
				.HasForeignKey(x => x.StudentId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Cohort)
				.WithMany()
				.HasForeignKey(x => x.CohortId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.AcademicYear)
				.WithMany()
				.HasForeignKey(x => x.AcademicYearId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => x.AcademicYearId);
			b.HasIndex(x => x.MembershipType);
		}
	}
}