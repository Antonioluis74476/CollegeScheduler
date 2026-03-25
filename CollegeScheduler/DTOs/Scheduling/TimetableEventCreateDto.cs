namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventCreateDto
{
	public int TermId { get; set; }
	public int ModuleId { get; set; }
	public int RoomId { get; set; }
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public int EventStatusId { get; set; }
	public string? SessionType { get; set; } // optional => Lecture
	public Guid? RecurrenceGroupId { get; set; }
	public string? Notes { get; set; }
}