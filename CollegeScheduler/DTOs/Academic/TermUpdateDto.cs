using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Academic;

public sealed class TermUpdateDto
{
	[Range(1, 10)]
	public int TermNumber { get; set; } = 1;

	[Required, StringLength(50)]
	public string Name { get; set; } = "";

	[Required]
	public DateTime StartDate { get; set; }

	[Required]
	public DateTime EndDate { get; set; }

	public bool IsActive { get; set; }
}
