using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Profiles;

public sealed class PasswordResetDto
{
	[Required, StringLength(100, MinimumLength = 6)]
	public string NewPassword { get; set; } = "";
}