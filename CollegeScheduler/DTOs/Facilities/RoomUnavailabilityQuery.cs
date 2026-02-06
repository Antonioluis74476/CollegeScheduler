namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomUnavailabilityQuery
{
	// Optional filters
	public DateTime? FromUtc { get; init; }
	public DateTime? ToUtc { get; init; }
	public int? ReasonTypeId { get; init; }

	// Paging
	public int Page { get; init; } = 1;
	public int PageSize { get; init; } = 20;

	// Sorting
	public string SortBy { get; init; } = "start"; // start/end/created
	public string SortDir { get; init; } = "asc";
}
