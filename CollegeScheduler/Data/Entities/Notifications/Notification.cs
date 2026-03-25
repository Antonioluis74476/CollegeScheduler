using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Notifications
{
	public class Notification : AuditableEntity
	{
		[Key]
		public long NotificationId { get; set; }

		[Required]
		public int NotificationTypeId { get; set; }
		public NotificationType? NotificationType { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Message { get; set; } = string.Empty;

		public long? RelatedTimetableEventId { get; set; }
		public TimetableEvent? RelatedTimetableEvent { get; set; }

		public long? RelatedRequestId { get; set; }
		public Request? RelatedRequest { get; set; }

		public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
	}
}