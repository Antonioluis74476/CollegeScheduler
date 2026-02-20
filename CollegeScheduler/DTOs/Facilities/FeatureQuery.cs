namespace CollegeScheduler.DTOs.Facilities;

public sealed class FeatureQuery
{
	public string? Search { get; init; }

	public int Page { get; init; } = 1;
	public int PageSize { get; init; } = 20;

	public string SortBy { get; init; } = "name";   // name
	public string SortDir { get; init; } = "asc";   // asc/desc
}
