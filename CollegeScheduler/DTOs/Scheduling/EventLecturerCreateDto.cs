namespace CollegeScheduler.DTOs.Scheduling;

public sealed class EventLecturerCreateDto
{
	public long TimetableEventId { get; set; }
	public int LecturerId { get; set; }
}