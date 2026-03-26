using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
	{
		public void Configure(EntityTypeBuilder<AcademicYear> b)
		{
			b.ToTable("AcademicYears");

			b.HasIndex(x => x.Label).IsUnique();
			b.HasIndex(x => new { x.IsCurrent, x.IsActive });

			b.Property(x => x.Label)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.StartDate)
				.HasColumnType("date")
				.IsRequired();

			b.Property(x => x.EndDate)
				.HasColumnType("date")
				.IsRequired();

			// EndDate > StartDate
			b.ToTable(t => t.HasCheckConstraint(
				"CK_AcademicYears_EndDateAfterStartDate",
				"[EndDate] > [StartDate]"
			));
		}
	}
}
