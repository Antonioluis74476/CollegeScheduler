using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortModuleCreateDto
{
	[Required]
	public int ModuleId { get; set; }

	[Required]
	public int TermId { get; set; }

	public bool IsRequired { get; set; } = true;
}
