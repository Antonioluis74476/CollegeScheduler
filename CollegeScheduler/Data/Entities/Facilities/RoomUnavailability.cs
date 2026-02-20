using CollegeScheduler.Data.Entities.Common;

namespace CollegeScheduler.Data.Entities.Facilities;

public class RoomUnavailability : AuditableEntity
{
	public int RoomUnavailabilityId { get; set; }

	public int RoomId { get; set; }
	public Room? Room { get; set; }

	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }

	public int UnavailabilityReasonTypeId { get; set; }
	public UnavailabilityReasonType? UnavailabilityReasonType { get; set; }

	public string? Notes { get; set; }

	// FK -> AspNetUsers (Identity)
	public string CreatedByUserId { get; set; } = "";
	public ApplicationUser? CreatedByUser { get; set; }
}
