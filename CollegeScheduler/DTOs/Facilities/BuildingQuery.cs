namespace CollegeScheduler.DTOs.Facilities;

public sealed class BuildingQuery
{
	public string? Search { get; init; }
	public string? Code { get; init; }
	public string? Faculty { get; init; }
	public bool? IsActive { get; init; }

	public int Page { get; init; } = 1;
	public int PageSize { get; init; } = 20;

	public string? SortBy { get; init; } = "name";   // name, code, faculty
	public string? SortDir { get; init; } = "asc";   // asc, desc
}
