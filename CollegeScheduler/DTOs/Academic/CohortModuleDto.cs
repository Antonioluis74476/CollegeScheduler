namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortModuleDto
{
	public int CohortModuleId { get; set; }

	public int CohortId { get; set; }
	public int ModuleId { get; set; }
	public int TermId { get; set; }

	public bool IsRequired { get; set; }
	public bool IsActive { get; set; }
}
