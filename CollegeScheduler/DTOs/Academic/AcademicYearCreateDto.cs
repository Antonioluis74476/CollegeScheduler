using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class AcademicYearCreateDto
{
	[Required, StringLength(20)]
	public string Label { get; set; } = "";

	[Required]
	public DateTime StartDate { get; set; }

	[Required]
	public DateTime EndDate { get; set; }

	public bool IsCurrent { get; set; } = false;
}
