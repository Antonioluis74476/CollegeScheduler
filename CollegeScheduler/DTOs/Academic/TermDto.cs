namespace CollegeScheduler.DTOs.Academic;

public sealed class TermDto
{
	public int TermId { get; set; }
	public int AcademicYearId { get; set; }
	public int TermNumber { get; set; }
	public string Name { get; set; } = "";
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public bool IsActive { get; set; }
}
