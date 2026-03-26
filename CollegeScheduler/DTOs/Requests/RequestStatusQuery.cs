namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestStatusQuery
{
	public string? Search { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "name";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}