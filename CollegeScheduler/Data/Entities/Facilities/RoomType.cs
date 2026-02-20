namespace CollegeScheduler.Data.Entities.Facilities;

public class RoomType
{
	public int RoomTypeId { get; set; }
	public string Name { get; set; } = "";

	public List<Room> Rooms { get; set; } = new();
}
