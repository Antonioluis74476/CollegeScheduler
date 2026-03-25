using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationTypeUpdateDto
{
	[Required]
	[StringLength(50)]
	public string Name { get; set; } = string.Empty;

	public bool IsActive { get; set; } = true;
}