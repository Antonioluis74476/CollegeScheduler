using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Profiles;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Profiles
{
	public class LecturerProfile : AuditableEntity
	{
		[Key]
		public int LecturerId { get; set; }

		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }

		[Required, StringLength(20)]
		public string StaffNumber { get; set; } = "";

		[Required, StringLength(200)]
		public string Name { get; set; } = "";

		[Required, StringLength(200)]
		public string LastName { get; set; } = "";

		[Required, StringLength(256)]
		public string Email { get; set; } = "";

		public int? DepartmentId { get; set; }

		[Required, StringLength(20)]
		public string EmploymentType { get; set; } = "FullTime";

		public int MaxWeeklyHours { get; set; } = 40;
		
	}
}
