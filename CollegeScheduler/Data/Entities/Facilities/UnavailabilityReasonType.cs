namespace CollegeScheduler.Data.Entities.Facilities;

public class UnavailabilityReasonType
{
	public int UnavailabilityReasonTypeId { get; set; }
	public string Name { get; set; } = "";

	public List<RoomUnavailability> RoomUnavailabilities { get; set; } = new();
}
