namespace CollegeScheduler.DTOs.Academic;

public sealed class ProgramDto
{
	public int ProgramId { get; set; }
	public int DepartmentId { get; set; }
	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public string Level { get; set; } = "";
	public int DurationYears { get; set; }
	public bool IsActive { get; set; }
}
