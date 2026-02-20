using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public class CohortModuleConfiguration : IEntityTypeConfiguration<CohortModule>
	{
		public void Configure(EntityTypeBuilder<CohortModule> b)
		{
			b.ToTable("CohortModules");

			// UNIQUE(CohortId, ModuleId, TermId)
			b.HasIndex(x => new { x.CohortId, x.ModuleId, x.TermId })
				.IsUnique();

			b.HasIndex(x => new { x.CohortId, x.IsActive });
			b.HasIndex(x => new { x.TermId, x.IsActive });
			b.HasIndex(x => new { x.ModuleId, x.IsActive });

			b.Property(x => x.IsRequired)
				.HasDefaultValue(true);

			b.HasOne(x => x.Cohort)
				.WithMany()
				.HasForeignKey(x => x.CohortId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Module)
				.WithMany()
				.HasForeignKey(x => x.ModuleId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Term)
				.WithMany()
				.HasForeignKey(x => x.TermId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
