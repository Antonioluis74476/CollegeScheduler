using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Facilities;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Requests
{
	public class RequestRoomBooking : AuditableEntity
	{
		[Key]
		public long RequestId { get; set; }
		public Request? Request { get; set; }

		[Required]
		public int RoomId { get; set; }
		public Room? Room { get; set; }

		[Required]
		public DateTime StartUtc { get; set; }

		[Required]
		public DateTime EndUtc { get; set; }

		[Required]
		[StringLength(300)]
		public string Purpose { get; set; } = string.Empty;

		public int ExpectedAttendees { get; set; } = 1;
	}
}