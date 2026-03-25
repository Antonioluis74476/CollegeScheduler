using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Notifications
{
	public class NotificationType : AuditableEntity
	{
		[Key]
		public int NotificationTypeId { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; } = string.Empty;
	}
}