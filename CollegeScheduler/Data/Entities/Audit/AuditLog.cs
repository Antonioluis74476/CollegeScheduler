using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Audit
{
	public class AuditLog : AuditableEntity
	{
		[Key]
		public long AuditLogId { get; set; }

		[StringLength(450)]
		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }

		[Required]
		[StringLength(20)]
		public string Action { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string EntityType { get; set; } = string.Empty;

		[StringLength(100)]
		public string? EntityId { get; set; }

		public string? OldValuesJson { get; set; }
		public string? NewValuesJson { get; set; }

		[StringLength(45)]
		public string? IpAddress { get; set; }

		[StringLength(300)]
		public string? UserAgent { get; set; }

		[Required]
		public DateTime PerformedAtUtc { get; set; }
	}
}