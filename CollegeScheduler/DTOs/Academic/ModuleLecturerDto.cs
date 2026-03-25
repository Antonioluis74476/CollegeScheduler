namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleLecturerDto
{
	public int ModuleId { get; set; }
	public int LecturerId { get; set; }
	public int TermId { get; set; }

	public string Role { get; set; } = "";
	public DateTime AssignedAtUtc { get; set; }
}