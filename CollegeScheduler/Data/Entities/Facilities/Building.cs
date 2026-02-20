using CollegeScheduler.Data.Entities.Common;

namespace CollegeScheduler.Data.Entities.Facilities;

public class Building : AuditableEntity
{
	public int BuildingId { get; set; }

	public int CampusId { get; set; }
	public Campus? Campus { get; set; }

	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Faculty { get; set; }

	public List<Room> Rooms { get; set; } = new();
}
