using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Scheduling;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class RequestScheduleChange : AuditableEntity
	{
		[Key]
		public long RequestId { get; set; }
		public Request? Request { get; set; }

		[Required]
		public long TimetableEventId { get; set; }
		public TimetableEvent? TimetableEvent { get; set; }

		public int? ProposedRoomId { get; set; }
		public Room? ProposedRoom { get; set; }

		public DateTime? ProposedStartUtc { get; set; }
		public DateTime? ProposedEndUtc { get; set; }

		[Required]
		[StringLength(800)]
		public string Reason { get; set; } = string.Empty;
	}
}