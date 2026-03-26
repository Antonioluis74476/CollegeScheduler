namespace CollegeScheduler.DTOs.Scheduling;

public sealed class EventLecturerQuery
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;

	public long? TimetableEventId { get; set; }
	public int? LecturerId { get; set; }
}