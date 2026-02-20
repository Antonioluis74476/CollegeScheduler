using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class BuildingUpdateDto
{
	[Required, StringLength(20)]
	public string Code { get; init; } = "";

	[Required, StringLength(200)]
	public string Name { get; init; } = "";

	[StringLength(120)]
	public string? Faculty { get; init; }

	public bool IsActive { get; init; }
}
