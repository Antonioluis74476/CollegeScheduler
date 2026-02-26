namespace CollegeScheduler.DTOs.Scheduling;

public sealed class TimetableEventChangeCreateDto
{
	public long TimetableEventId { get; set; }
	public string ChangeType { get; set; } = "";
	public int? OldRoomId { get; set; }
	public int? NewRoomId { get; set; }
	public DateTime? OldStartUtc { get; set; }
	public DateTime? NewStartUtc { get; set; }
	public DateTime? OldEndUtc { get; set; }
	public DateTime? NewEndUtc { get; set; }
	public string Reason { get; set; } = "";
	public bool? NotificationSent { get; set; }
}