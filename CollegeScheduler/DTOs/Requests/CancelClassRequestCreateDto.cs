namespace CollegeScheduler.DTOs.Requests;

public sealed class CancelClassRequestCreateDto
{
	public long TimetableEventId { get; set; }
	public string Reason { get; set; } = string.Empty;
}