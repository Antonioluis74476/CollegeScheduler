using Microsoft.AspNetCore.Identity;
using CollegeScheduler.Data.Entities.Profiles;

namespace CollegeScheduler.Data
{
	public class ApplicationUser : IdentityUser
	{
		// Optional 1–1 links to domain profiles
		public StudentProfile? StudentProfile { get; set; }
		public LecturerProfile? LecturerProfile { get; set; }
	}
}
