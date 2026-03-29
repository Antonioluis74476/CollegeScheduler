namespace CollegeScheduler.DTOs.Scheduling;

public sealed class ClashCheckRequest
{
	public long? ExcludeEventId { get; set; }
	public int RoomId { get; set; }
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public List<int> CohortIds { get; set; } = new();
	public List<int> LecturerIds { get; set; } = new();
}