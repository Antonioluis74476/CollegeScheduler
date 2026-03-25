using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationRecipientCreateDto
{
	[Required]
	public string UserId { get; set; } = string.Empty;

	[StringLength(20)]
	public string DeliveryStatus { get; set; } = "Pending";
}