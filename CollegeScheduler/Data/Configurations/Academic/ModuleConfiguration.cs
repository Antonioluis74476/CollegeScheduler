using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class ModuleConfiguration : IEntityTypeConfiguration<Module>
	{
		public void Configure(EntityTypeBuilder<Module> b)
		{
			b.ToTable("Modules");

			// Unique module code (global)
			b.HasIndex(x => x.Code).IsUnique();

			// Useful filters
			b.HasIndex(x => new { x.DepartmentId, x.IsActive });

			b.Property(x => x.Code)
				.HasMaxLength(20)
				.IsRequired();

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.HoursPerWeek)
				.HasColumnType("decimal(4,2)");

			b.Property(x => x.MinRoomCapacity)
				.HasDefaultValue(0);

			// Optional FK: DepartmentId can be null
			b.HasOne(x => x.Department)
				.WithMany()
				.HasForeignKey(x => x.DepartmentId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
