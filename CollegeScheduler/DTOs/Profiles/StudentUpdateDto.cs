using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Profiles;

public sealed class StudentUpdateDto
{
	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Required, StringLength(200)]
	public string LastName { get; set; } = "";

	[StringLength(20)]
	public string Status { get; set; } = "Active";

	public bool IsActive { get; set; } = true;
}
