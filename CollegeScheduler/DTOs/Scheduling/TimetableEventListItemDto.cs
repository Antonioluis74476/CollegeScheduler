namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventListItemDto
{
	public long TimetableEventId { get; set; }
	public Guid? RecurrenceGroupId { get; set; }
	public string ModuleCode { get; set; } = "";
	public string ModuleTitle { get; set; } = "";
	public string RoomCode { get; set; } = "";
	public string RoomName { get; set; } = "";
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public string SessionType { get; set; } = "";
	public string EventStatus { get; set; } = "";
	public List<string> CohortCodes { get; set; } = new();
	public List<string> LecturerNames { get; set; } = new();
}