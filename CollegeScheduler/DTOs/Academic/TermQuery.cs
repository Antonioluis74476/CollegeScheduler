namespace CollegeScheduler.DTOs.Academic;

public sealed class TermQuery
{
	public string? Search { get; set; } // matches Name
	public int? TermNumber { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "termNumber";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
