namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestScheduleChangeQuery
{
	public long? RequestId { get; set; }
	public long? TimetableEventId { get; set; }
	public int? ProposedRoomId { get; set; }
	public bool? IsActive { get; set; }

	// sortBy: timetableEventId / proposedStartUtc / proposedEndUtc / createdAt
	public string? SortBy { get; set; } = "createdAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}