namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationRecipientDto
{
	public long NotificationId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string DeliveryStatus { get; set; } = string.Empty;
	public DateTime? SentAtUtc { get; set; }
	public DateTime? ReadAtUtc { get; set; }
}