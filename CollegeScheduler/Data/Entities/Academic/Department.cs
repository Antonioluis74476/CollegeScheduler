using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class Department : AuditableEntity
	{
		[Key]
		public int DepartmentId { get; set; }

		[Required, StringLength(20)]
		public string Code { get; set; } = "";

		[Required, StringLength(200)]
		public string Name { get; set; } = "";

		[StringLength(100)]
		public string? Email { get; set; }
	}
}
