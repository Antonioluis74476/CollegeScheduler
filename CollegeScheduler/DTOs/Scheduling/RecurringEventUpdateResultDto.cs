namespace CollegeScheduler.DTOs.Scheduling;

public sealed class RecurringEventUpdateResultDto
{
	public bool Success { get; set; }
	public int UpdatedCount { get; set; }
	public List<long> EventIds { get; set; } = new();
	public List<OccurrenceClashDto>? Clashes { get; set; }
}

public sealed class OccurrenceClashDto
{
	public long TimetableEventId { get; set; }
	public ClashResult Clash { get; set; } = null!;
}