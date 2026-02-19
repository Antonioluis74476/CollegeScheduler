namespace CollegeScheduler.DTOs.Academic;

public sealed class AcademicYearDto
{
	public int AcademicYearId { get; set; }
	public string Label { get; set; } = "";
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public bool IsCurrent { get; set; }
	public bool IsActive { get; set; }
}
