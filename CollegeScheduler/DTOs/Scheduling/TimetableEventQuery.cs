namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventQuery
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;

	public int? TermId { get; set; }
	public int? ModuleId { get; set; }
	public int? RoomId { get; set; }
	public int? EventStatusId { get; set; }
	public Guid? RecurrenceGroupId { get; set; }

	public DateTime? FromUtc { get; set; }
	public DateTime? ToUtc { get; set; }
}