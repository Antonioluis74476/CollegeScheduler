namespace CollegeScheduler.DTOs.Academic;

public sealed class DepartmentDto
{
	public int DepartmentId { get; set; }
	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Email { get; set; }
	public bool IsActive { get; set; }
}
