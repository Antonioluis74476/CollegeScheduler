using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortModuleUpdateDto
{
	[Required]
	public int ModuleId { get; set; }

	[Required]
	public int TermId { get; set; }

	public bool IsRequired { get; set; } = true;

	public bool IsActive { get; set; } = true;
}
