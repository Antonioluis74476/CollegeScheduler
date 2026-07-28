namespace CollegeScheduler.DTOs.Scheduling;

public sealed class RecurringEventSeriesDto
{
	public Guid RecurrenceGroupId { get; set; }
	public List<TimetableEventDto> Occurrences { get; set; } = new();
}