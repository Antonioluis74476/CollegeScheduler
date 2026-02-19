namespace CollegeScheduler.DTOs.Academic;

public sealed class AcademicYearQuery
{
	public string? Search { get; set; } // matches Label
	public bool? IsCurrent { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "label";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
