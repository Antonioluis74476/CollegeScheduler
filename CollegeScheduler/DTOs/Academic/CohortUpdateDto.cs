using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortUpdateDto
{
	[Required]
	public int AcademicYearId { get; set; }

	[Range(1, 10)]
	public int YearOfStudy { get; set; } = 1;

	[Required, StringLength(20)]
	public string Code { get; set; } = "";

	[Required, StringLength(200)]
	public string Name { get; set; } = "";

	[Range(0, 10000)]
	public int ExpectedSize { get; set; } = 0;

	public bool IsActive { get; set; }
}
