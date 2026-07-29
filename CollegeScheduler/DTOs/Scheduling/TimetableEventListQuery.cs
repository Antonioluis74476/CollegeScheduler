namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventListQuery
{
	public int? TermId { get; set; }
	public int? CohortId { get; set; }
	public int? LecturerId { get; set; }
	public int? RoomId { get; set; }
	public int? ModuleId { get; set; }
	public DateTime? FromUtc { get; set; }
	public DateTime? ToUtc { get; set; }
	public bool IncludeCancelled { get; set; } = false;
}