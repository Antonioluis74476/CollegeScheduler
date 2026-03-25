namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationRecipientQuery
{
	public long? NotificationId { get; set; }
	public string? UserId { get; set; }
	public string? DeliveryStatus { get; set; }
	public bool? IsActive { get; set; }

	// sortBy: notificationId / userId / deliveryStatus / sentAt / readAt / createdAt
	public string? SortBy { get; set; } = "createdAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}