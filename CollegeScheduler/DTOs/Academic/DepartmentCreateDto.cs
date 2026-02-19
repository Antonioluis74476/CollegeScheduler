using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class DepartmentCreateDto
{
	[Required, StringLength(20)]
	public string Code { get; set; } = "";

	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[StringLength(100)]
	public string? Email { get; set; }
}
