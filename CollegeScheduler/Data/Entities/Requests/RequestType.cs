using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class RequestType : AuditableEntity
	{
		[Key]
		public int RequestTypeId { get; set; }

		[Required]
		[StringLength(30)]
		public string Name { get; set; } = string.Empty;
	}
}