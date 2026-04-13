namespace CollegeScheduler.DTOs.Scheduling;

public sealed record ClashResult
{
	public ClashDetail? RoomClash { get; set; }
	public List<ClashDetail> CohortClashes { get; } = new();
	public List<ClashDetail> LecturerClashes { get; } = new();

	public bool HasClash =>
		RoomClash is not null ||
		CohortClashes.Count > 0 ||
		LecturerClashes.Count > 0;
}

public sealed record ClashDetail(long ConflictingEventId, string Message);