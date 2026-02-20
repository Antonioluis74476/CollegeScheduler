using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class Cohort : AuditableEntity
	{
		[Key]
		public int CohortId { get; set; }

		[Required]
		public int ProgramId { get; set; }
		public AcademicProgram? Program { get; set; }

		[Required]
		public int AcademicYearId { get; set; }
		public AcademicYear? AcademicYear { get; set; }

		[Required]
		public int YearOfStudy { get; set; }

		[Required, StringLength(20)]
		public string Code { get; set; } = ""; // A / B / CS2-A

		[Required, StringLength(200)]
		public string Name { get; set; } = ""; // "CS Year 2 Group A"

		public int ExpectedSize { get; set; } = 0;
	}
}
