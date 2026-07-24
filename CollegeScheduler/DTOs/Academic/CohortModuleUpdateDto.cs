using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortModuleUpdateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a module.")]
    public int ModuleId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a term.")]
    public int TermId { get; set; }

    public bool IsRequired { get; set; } = true;

	public bool IsActive { get; set; } = true;
}
