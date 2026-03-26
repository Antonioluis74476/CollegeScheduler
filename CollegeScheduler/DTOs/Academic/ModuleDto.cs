namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleDto
{
	public int ModuleId { get; set; }
	public int? DepartmentId { get; set; }

	public string Code { get; set; } = "";
	public string Title { get; set; } = "";

	public int Credits { get; set; }
	public decimal? HoursPerWeek { get; set; }

	public int MinRoomCapacity { get; set; }
	public bool IsActive { get; set; }
}
