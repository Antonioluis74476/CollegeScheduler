namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleLecturerQuery
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;

	public int? ModuleId { get; set; }
	public int? LecturerId { get; set; }
	public int? TermId { get; set; }
	public string? Role { get; set; }
}