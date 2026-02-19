using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class CohortModule : AuditableEntity
	{
		[Key]
		public int CohortModuleId { get; set; }

		[Required]
		public int CohortId { get; set; }
		public Cohort? Cohort { get; set; }

		[Required]
		public int ModuleId { get; set; }
		public Module? Module { get; set; }

		[Required]
		public int TermId { get; set; }
		public Term? Term { get; set; }

		public bool IsRequired { get; set; } = true;
	}
}
