namespace CollegeScheduler.DTOs.Lecturer;

public sealed class LecturerTimetableItemDto
{
	public long TimetableEventId { get; set; }

	public DateTime StartUtc { get; set; }

	public DateTime EndUtc { get; set; }

	public string SessionType { get; set; } = "";

    public int ModuleId { get; set; }
    public string ModuleCode { get; set; } = "";
    public string ModuleTitle { get; set; } = "";

    public int RoomId { get; set; }
    public string RoomCode { get; set; } = "";
    public string? RoomName { get; set; }

    public int StatusId { get; set; }
    public string StatusName { get; set; } = "";

    public string? Notes { get; set; }
}