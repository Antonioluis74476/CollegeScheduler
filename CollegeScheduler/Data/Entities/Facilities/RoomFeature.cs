namespace CollegeScheduler.Data.Entities.Facilities;

public class RoomFeature
{
	public int RoomId { get; set; }
	public Room? Room { get; set; }

	public int FeatureId { get; set; }
	public Feature? Feature { get; set; }
}
