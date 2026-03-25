namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestRoomBookingQuery
{
	public int? RoomId { get; set; }
	public long? RequestId { get; set; }
	public bool? IsActive { get; set; }

	// sortBy: roomId / startUtc / endUtc / createdAt
	public string? SortBy { get; set; } = "startUtc";
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}