namespace CollegeScheduler.DTOs.Scheduling;

public sealed class AdminEventRescheduleDto
{
	public int RoomId { get; set; }
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public string? Reason { get; set; }
}