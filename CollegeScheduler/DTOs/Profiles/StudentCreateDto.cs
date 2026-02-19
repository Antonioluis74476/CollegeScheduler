using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Profiles;

public sealed class StudentCreateDto
{
	// Identity
	[Required, EmailAddress, StringLength(256)]
	public string Email { get; set; } = "";

	[Required, StringLength(100, MinimumLength = 6)]
	public string Password { get; set; } = "";

	// Profile
	[Required, StringLength(20)]
	public string StudentNumber { get; set; } = "";

	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Required, StringLength(200)]
	public string LastName { get; set; } = "";

	[StringLength(20)]
	public string Status { get; set; } = "Active";
}
