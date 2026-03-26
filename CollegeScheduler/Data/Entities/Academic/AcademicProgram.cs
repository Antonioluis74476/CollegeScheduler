using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class AcademicProgram : AuditableEntity
	{
		[Key]
		public int ProgramId { get; set; }

		[Required]
		public int DepartmentId { get; set; }
		public Department? Department { get; set; }

		[Required, StringLength(20)]
		public string Code { get; set; } = "";

		[Required, StringLength(200)]
		public string Name { get; set; } = "";

		[Required, StringLength(20)]
		public string Level { get; set; } = "Bachelor";

		public int DurationYears { get; set; } = 4;
	}
}
