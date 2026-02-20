using CollegeScheduler.Data.Entities.Common;
using Microsoft.AspNetCore.Http.Features;

namespace CollegeScheduler.Data.Entities.Facilities;

public class Room : AuditableEntity
{
	public int RoomId { get; set; }

	public int BuildingId { get; set; }
	public Building? Building { get; set; }

	public int RoomTypeId { get; set; }
	public RoomType? RoomType { get; set; }

	public string Code { get; set; } = "";
	public string? Name { get; set; }
	public string? Floor { get; set; }

	public int Capacity { get; set; } = 0;
	public bool IsBookableByStudents { get; set; } = false;
	public bool RequiresApproval { get; set; } = true;
	public string? Notes { get; set; }

	public List<RoomFeature> RoomFeatures { get; set; } = new();
	public List<RoomUnavailability> Unavailabilities { get; set; } = new();
}
