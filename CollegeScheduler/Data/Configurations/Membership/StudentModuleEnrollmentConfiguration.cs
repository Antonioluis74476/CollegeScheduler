using CollegeScheduler.Data.Entities.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Membership
{
	public sealed class StudentModuleEnrollmentConfiguration : IEntityTypeConfiguration<StudentModuleEnrollment>
	{
		public void Configure(EntityTypeBuilder<StudentModuleEnrollment> b)
		{
			b.ToTable("StudentModuleEnrollments");

			b.HasKey(x => new { x.StudentId, x.ModuleId, x.TermId });

			b.Property(x => x.EnrollmentType)
				.IsRequired()
				.HasMaxLength(20);

			b.Property(x => x.Status)
				.IsRequired()
				.HasMaxLength(20)
				.HasDefaultValue("Enrolled");

			b.Property(x => x.EnrolledAtUtc)
				.IsRequired()
				.HasColumnType("datetime2");

			b.HasOne(x => x.Student)
				.WithMany()
				.HasForeignKey(x => x.StudentId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Module)
				.WithMany()
				.HasForeignKey(x => x.ModuleId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Term)
				.WithMany()
				.HasForeignKey(x => x.TermId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.AttendWithCohort)
				.WithMany()
				.HasForeignKey(x => x.AttendWithCohortId)
				.OnDelete(DeleteBehavior.Restrict);

			// Required indexes
			b.HasIndex(x => x.AttendWithCohortId);
			b.HasIndex(x => x.Status);

			// Helpful indexes
			b.HasIndex(x => x.TermId);
			b.HasIndex(x => x.ModuleId);
		}
	}
}