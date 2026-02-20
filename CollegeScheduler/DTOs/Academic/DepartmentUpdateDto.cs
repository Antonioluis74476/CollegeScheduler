using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class DepartmentUpdateDto
{
	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[StringLength(100)]
	public string? Email { get; set; }

	public bool IsActive { get; set; }
}
