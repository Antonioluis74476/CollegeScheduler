namespace CollegeScheduler.DTOs.Scheduling;

public sealed class RecurringEventCreateDto
{
	public int TermId { get; set; }
	public int ModuleId { get; set; }
	public int RoomId { get; set; }
	public DateTime FirstOccurrenceStartUtc { get; set; }
	public DateTime FirstOccurrenceEndUtc { get; set; }
	public int EventStatusId { get; set; }
	public string SessionType { get; set; } = "Lecture";
	public string? Notes { get; set; }
	public List<int> CohortIds { get; set; } = new();
	public List<int> LecturerIds { get; set; } = new();
	public HashSet<DateOnly>? ExcludeDates { get; set; }
}