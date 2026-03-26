using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class DepartmentConfiguration : IEntityTypeConfiguration<AcademicProgram>
	{
		public void Configure(EntityTypeBuilder<AcademicProgram> b)
		{
			b.ToTable("Programs");

			b.HasIndex(x => x.Code).IsUnique();
			b.HasIndex(x => new { x.DepartmentId, x.IsActive });

			b.Property(x => x.Code).HasMaxLength(20).IsRequired();
			b.Property(x => x.Name).HasMaxLength(200).IsRequired();
			b.Property(x => x.Level).HasMaxLength(20).IsRequired();

			b.HasOne(x => x.Department)
				.WithMany()
				.HasForeignKey(x => x.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
