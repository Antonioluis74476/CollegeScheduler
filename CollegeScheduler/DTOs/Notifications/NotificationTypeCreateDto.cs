using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Notifications;

public sealed class NotificationTypeCreateDto
{
	[Required]
	[StringLength(50)]
	public string Name { get; set; } = string.Empty;
}