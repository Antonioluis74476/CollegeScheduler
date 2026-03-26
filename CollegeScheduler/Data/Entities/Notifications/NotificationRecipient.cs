using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Notifications
{
	public class NotificationRecipient : AuditableEntity
	{
		[Required]
		public long NotificationId { get; set; }
		public Notification? Notification { get; set; }

		[Required]
		[StringLength(450)]
		public string UserId { get; set; } = string.Empty;
		public ApplicationUser? User { get; set; }

		[Required]
		[StringLength(20)]
		public string DeliveryStatus { get; set; } = "Pending";

		public DateTime? SentAtUtc { get; set; }
		public DateTime? ReadAtUtc { get; set; }
	}
}