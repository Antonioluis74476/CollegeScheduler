using CollegeScheduler.Data.Entities.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Academic
{
	public sealed class ModuleLecturerConfiguration : IEntityTypeConfiguration<ModuleLecturer>
	{
		public void Configure(EntityTypeBuilder<ModuleLecturer> b)
		{
			b.ToTable("ModuleLecturers");

			b.HasKey(x => new { x.ModuleId, x.LecturerId, x.TermId });

			b.Property(x => x.Role)
				.IsRequired()
				.HasMaxLength(20)
				.HasDefaultValue("Lead");

			b.Property(x => x.AssignedAtUtc)
				.IsRequired()
				.HasColumnType("datetime2");

			b.HasOne(x => x.Module)
				.WithMany()
				.HasForeignKey(x => x.ModuleId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Lecturer)
				.WithMany()
				.HasForeignKey(x => x.LecturerId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Term)
				.WithMany()
				.HasForeignKey(x => x.TermId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => x.TermId);
			b.HasIndex(x => x.LecturerId);
		}
	}
}