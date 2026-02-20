namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortDto
{
	public int CohortId { get; set; }
	public int ProgramId { get; set; }
	public int AcademicYearId { get; set; }
	public int YearOfStudy { get; set; }
	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public int ExpectedSize { get; set; }
	public bool IsActive { get; set; }
}
