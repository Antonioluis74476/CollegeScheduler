using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class Module : AuditableEntity
	{
		[Key]
		public int ModuleId { get; set; }

		// Optional: Module can belong to a Department (or be shared across departments)
		public int? DepartmentId { get; set; }
		public Department? Department { get; set; }

		[Required, StringLength(20)]
		public string Code { get; set; } = "";

		[Required, StringLength(200)]
		public string Title { get; set; } = "";

		[Required]
		public int Credits { get; set; }

		// Optional weekly hours (e.g. 3.50)
		public decimal? HoursPerWeek { get; set; } // decimal(4,2)

		// Minimum room capacity needed (0 means no requirement)
		public int MinRoomCapacity { get; set; } = 0;
	}
}
