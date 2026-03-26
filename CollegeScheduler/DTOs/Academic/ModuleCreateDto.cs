using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleCreateDto
{
	public int? DepartmentId { get; set; }

	[Required, StringLength(20)]
	public string Code { get; set; } = "";

	[Required, StringLength(200)]
	public string Title { get; set; } = "";

	[Range(0, 120)]
	public int Credits { get; set; }

	public decimal? HoursPerWeek { get; set; }

	[Range(0, 10000)]
	public int MinRoomCapacity { get; set; } = 0;
}
