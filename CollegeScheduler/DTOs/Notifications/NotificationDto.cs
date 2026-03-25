namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationDto
{
	public long NotificationId { get; set; }
	public int NotificationTypeId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public long? RelatedTimetableEventId { get; set; }
	public long? RelatedRequestId { get; set; }
	public bool IsActive { get; set; }

	public List<NotificationRecipientDto> Recipients { get; set; } = new();
}