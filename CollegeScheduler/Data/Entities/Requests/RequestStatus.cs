using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class RequestStatus : AuditableEntity
	{
		[Key]
		public int RequestStatusId { get; set; }

		[Required]
		[StringLength(20)]
		public string Name { get; set; } = string.Empty;
	}
}