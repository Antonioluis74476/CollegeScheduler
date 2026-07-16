namespace CollegeScheduler.DTOs.Student;

public sealed class StudentTimetableItemDto
{
	public long TimetableEventId { get; set; }

	public int CohortId { get; set; }

	public DateTime StartUtc { get; set; }

	public DateTime EndUtc { get; set; }

	public string SessionType { get; set; } = "";

	public int ModuleId { get; set; }

	public int RoomId { get; set; }

	public int StatusId { get; set; }

	public string? Notes { get; set; }
}