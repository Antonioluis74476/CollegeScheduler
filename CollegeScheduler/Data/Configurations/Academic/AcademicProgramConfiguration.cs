using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class AcademicProgramConfiguration
		: IEntityTypeConfiguration<AcademicProgram>
	{
		public void Configure(EntityTypeBuilder<AcademicProgram> b)
		{
			b.ToTable("Programs");

			// Unique program code (global)
			b.HasIndex(x => x.Code)
				.IsUnique();

			// Common filtering index
			b.HasIndex(x => new { x.DepartmentId, x.IsActive });

			b.Property(x => x.Code)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Level)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.DurationYears)
				.HasDefaultValue(4);

			// FK → Department
			b.HasOne(x => x.Department)
				.WithMany()
				.HasForeignKey(x => x.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
