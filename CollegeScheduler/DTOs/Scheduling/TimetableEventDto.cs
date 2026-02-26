namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventDto
{
	public long TimetableEventId { get; set; }
	public int TermId { get; set; }
	public int ModuleId { get; set; }
	public int RoomId { get; set; }
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public int EventStatusId { get; set; }
	public string SessionType { get; set; } = "";
	public Guid? RecurrenceGroupId { get; set; }
	public string? Notes { get; set; }
	public string CreatedByUserId { get; set; } = "";
	public DateTime CreatedAtUtc { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
}