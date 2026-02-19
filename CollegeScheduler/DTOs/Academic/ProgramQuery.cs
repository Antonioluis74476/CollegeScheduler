namespace CollegeScheduler.DTOs.Academic;

public sealed class ProgramQuery
{
	public string? Search { get; set; }
	public string? Code { get; set; }
	public string? Level { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "code";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
