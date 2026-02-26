using CollegeScheduler.Data.Entities.Profiles;

namespace CollegeScheduler.Data.Entities.Scheduling;

public sealed class EventLecturer
{
	public long TimetableEventId { get; set; }
	public int LecturerId { get; set; }

	public TimetableEvent TimetableEvent { get; set; } = null!;
	public LecturerProfile Lecturer { get; set; } = null!;
}