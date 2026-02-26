namespace CollegeScheduler.DTOs.Scheduling;

public sealed class EventCohortCreateDto
{
	public long TimetableEventId { get; set; }
	public int CohortId { get; set; }
}