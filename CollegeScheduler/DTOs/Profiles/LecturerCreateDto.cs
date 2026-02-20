using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Profiles;

public sealed class LecturerCreateDto
{
	// Identity
	[Required, EmailAddress, StringLength(256)]
	public string Email { get; set; } = "";

	[Required, StringLength(100, MinimumLength = 6)]
	public string Password { get; set; } = "";

	// Profile
	[Required, StringLength(20)]
	public string StaffNumber { get; set; } = "";

	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Required, StringLength(200)]
	public string LastName { get; set; } = "";

	public int? DepartmentId { get; set; }

	[StringLength(20)]
	public string EmploymentType { get; set; } = "FullTime";

	[Range(1, 80)]
	public int MaxWeeklyHours { get; set; } = 40;
}
