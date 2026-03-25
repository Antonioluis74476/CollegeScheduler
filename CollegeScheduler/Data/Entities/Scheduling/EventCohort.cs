using CollegeScheduler.Data.Entities.Academic;

namespace CollegeScheduler.Data.Entities.Scheduling;

public sealed class EventCohort
{
	public long TimetableEventId { get; set; }
	public int CohortId { get; set; }

	public TimetableEvent TimetableEvent { get; set; } = null!;
	public Cohort Cohort { get; set; } = null!;
}