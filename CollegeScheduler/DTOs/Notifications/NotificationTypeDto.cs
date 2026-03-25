namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationTypeDto
{
	public int NotificationTypeId { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
}