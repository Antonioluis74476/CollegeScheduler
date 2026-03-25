namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationQuery
{
	public string? Search { get; set; }
	public int? NotificationTypeId { get; set; }
	public long? RelatedTimetableEventId { get; set; }
	public long? RelatedRequestId { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "createdAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}