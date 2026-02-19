using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Profiles;

public sealed class LecturerUpdateDto
{
	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Required, StringLength(200)]
	public string LastName { get; set; } = "";

	public int? DepartmentId { get; set; }

	[StringLength(20)]
	public string EmploymentType { get; set; } = "FullTime";

	[Range(1, 80)]
	public int MaxWeeklyHours { get; set; } = 40;

	public bool IsActive { get; set; } = true;
}
