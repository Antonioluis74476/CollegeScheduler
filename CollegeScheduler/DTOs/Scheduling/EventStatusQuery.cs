namespace CollegeScheduler.DTOs.Scheduling;

public sealed class EventStatusQuery
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
	public string? Search { get; set; }
}