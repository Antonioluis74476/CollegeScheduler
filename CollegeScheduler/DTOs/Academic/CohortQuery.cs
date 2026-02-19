namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortQuery
{
	public string? Search { get; set; } // matches Code/Name
	public int? AcademicYearId { get; set; }
	public int? YearOfStudy { get; set; }
	public string? Code { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "code";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
