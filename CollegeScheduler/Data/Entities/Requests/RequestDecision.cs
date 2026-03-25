using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class RequestDecision : AuditableEntity
	{
		[Key]
		public long RequestDecisionId { get; set; }

		[Required]
		public long RequestId { get; set; }
		public Request? Request { get; set; }

		[Required]
		[StringLength(450)]
		public string DecidedByUserId { get; set; } = string.Empty;
		public ApplicationUser? DecidedByUser { get; set; }

		[Required]
		[StringLength(20)]
		public string Decision { get; set; } = string.Empty; // Approved / Rejected

		[StringLength(500)]
		public string? Comment { get; set; }

		[Required]
		public DateTime DecidedAtUtc { get; set; }
	}
}