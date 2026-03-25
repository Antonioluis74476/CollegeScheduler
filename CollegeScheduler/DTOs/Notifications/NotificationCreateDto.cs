using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationCreateDto
{
	[Required]
	public int NotificationTypeId { get; set; }

	[Required]
	[StringLength(200)]
	public string Title { get; set; } = string.Empty;

	[Required]
	public string Message { get; set; } = string.Empty;

	public long? RelatedTimetableEventId { get; set; }
	public long? RelatedRequestId { get; set; }

	public List<NotificationRecipientCreateDto> Recipients { get; set; } = new();
}