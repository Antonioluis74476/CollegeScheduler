namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleLecturerCreateDto
{
	public int ModuleId { get; set; }
	public int LecturerId { get; set; }
	public int TermId { get; set; }

	// Optional → defaults to "Lead"
	public string? Role { get; set; }
}