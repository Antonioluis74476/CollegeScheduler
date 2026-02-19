using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Profiles;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Profiles
{
	public class StudentProfile : AuditableEntity
	{
		[Key]
		public int StudentId { get; set; }

		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }

		[Required, StringLength(20)]
		public string StudentNumber { get; set; } = "";

		[Required, StringLength(200)]
		public string Name { get; set; } = "";

		[Required, StringLength(200)]
		public string LastName { get; set; } = "";

		[Required, StringLength(256)]
		public string Email { get; set; } = "";

		[Required, StringLength(20)]
		public string Status { get; set; } = "Active";
	}
}
