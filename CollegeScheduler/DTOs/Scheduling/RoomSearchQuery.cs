namespace CollegeScheduler.DTOs.Scheduling;

public sealed class RoomSearchQuery
{
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public int? MinCapacity { get; set; }
	public int? RoomTypeId { get; set; }
	public int? BuildingId { get; set; }
	public int? CampusId { get; set; }
	public List<int>? RequiredFeatureIds { get; set; }
}