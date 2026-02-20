using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class TermConfiguration : IEntityTypeConfiguration<Term>
	{
		public void Configure(EntityTypeBuilder<Term> b)
		{
			b.ToTable("Terms");

			// UNIQUE(AcademicYearId, TermNumber)
			b.HasIndex(x => new { x.AcademicYearId, x.TermNumber })
				.IsUnique();

			b.HasIndex(x => new { x.AcademicYearId, x.IsActive });

			b.Property(x => x.Name)
				.HasMaxLength(50)
				.IsRequired();

			b.Property(x => x.StartDate)
				.HasColumnType("date")
				.IsRequired();

			b.Property(x => x.EndDate)
				.HasColumnType("date")
				.IsRequired();

			// EndDate > StartDate
			b.ToTable(t => t.HasCheckConstraint(
				"CK_Terms_EndDateAfterStartDate",
				"[EndDate] > [StartDate]"
			));

			b.HasOne(x => x.AcademicYear)
				.WithMany()
				.HasForeignKey(x => x.AcademicYearId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
