namespace CollegeScheduler.DTOs.Requests;

public sealed class ScheduleChangeRequestCreateDto
{
	public long TimetableEventId { get; set; }
	public int? ProposedRoomId { get; set; }
	public DateTime? ProposedStartUtc { get; set; }
	public DateTime? ProposedEndUtc { get; set; }
	public string Reason { get; set; } = string.Empty;
}