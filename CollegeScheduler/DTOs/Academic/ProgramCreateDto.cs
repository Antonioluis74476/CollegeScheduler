using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class ProgramCreateDto
{
	[Required, StringLength(20)]
	public string Code { get; set; } = "";

	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Required, StringLength(20)]
	public string Level { get; set; } = "Bachelor";

	[Range(1, 20)]
	public int DurationYears { get; set; } = 4;
}
