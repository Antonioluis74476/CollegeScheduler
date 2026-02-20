using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class CohortConfiguration : IEntityTypeConfiguration<Cohort>
	{
		public void Configure(EntityTypeBuilder<Cohort> b)
		{
			b.ToTable("Cohorts");

			// UNIQUE(ProgramId, AcademicYearId, YearOfStudy, Code)
			b.HasIndex(x => new { x.ProgramId, x.AcademicYearId, x.YearOfStudy, x.Code })
				.IsUnique();

			b.HasIndex(x => new { x.AcademicYearId, x.IsActive });
			b.HasIndex(x => new { x.ProgramId, x.IsActive });

			b.Property(x => x.Code)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.ExpectedSize)
				.HasDefaultValue(0);

			b.HasOne(x => x.Program)
				.WithMany()
				.HasForeignKey(x => x.ProgramId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.AcademicYear)
				.WithMany()
				.HasForeignKey(x => x.AcademicYearId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
