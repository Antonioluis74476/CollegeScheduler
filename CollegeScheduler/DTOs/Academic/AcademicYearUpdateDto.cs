using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class AcademicYearUpdateDto
{
	[Required]
	public DateTime StartDate { get; set; }

	[Required]
	public DateTime EndDate { get; set; }

	public bool IsCurrent { get; set; }
	public bool IsActive { get; set; }
}
