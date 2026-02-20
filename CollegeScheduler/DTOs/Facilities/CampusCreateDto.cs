using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class CampusCreateDto
{
	[Required, StringLength(20)]
	public string Code { get; init; } = "";

	[Required, StringLength(200)]
	public string Name { get; init; } = "";

	[StringLength(300)]
	public string? Address { get; init; }

	[StringLength(120)]
	public string? City { get; init; }
}
