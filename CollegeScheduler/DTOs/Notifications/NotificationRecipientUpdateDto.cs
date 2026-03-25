using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationRecipientUpdateDto
{
	[Required]
	[StringLength(20)]
	public string DeliveryStatus { get; set; } = "Pending";

	public DateTime? SentAtUtc { get; set; }
	public DateTime? ReadAtUtc { get; set; }

	public bool IsActive { get; set; } = true;
}