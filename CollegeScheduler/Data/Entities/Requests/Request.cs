using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class Request : AuditableEntity
	{
		[Key]
		public long RequestId { get; set; }

		[Required]
		public int RequestTypeId { get; set; }
		public RequestType? RequestType { get; set; }

		[Required]
		public int RequestStatusId { get; set; }
		public RequestStatus? RequestStatus { get; set; }

		[Required]
		[StringLength(450)]
		public string RequestedByUserId { get; set; } = string.Empty;
		public ApplicationUser? RequestedByUser { get; set; }

		[StringLength(200)]
		public string? Title { get; set; }

		public string? Notes { get; set; }

		public RequestRoomBooking? RoomBookingDetail { get; set; }
		public RequestScheduleChange? ScheduleChangeDetail { get; set; }

		public ICollection<RequestDecision> Decisions { get; set; } = new List<RequestDecision>();
	}
}