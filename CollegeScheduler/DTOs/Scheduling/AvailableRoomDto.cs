namespace CollegeScheduler.DTOs.Scheduling;

public sealed class AvailableRoomDto
{
	public int RoomId { get; set; }
	public string Code { get; set; } = string.Empty;
	public string? Name { get; set; }
	public int Capacity { get; set; }
	public string BuildingName { get; set; } = string.Empty;
	public string CampusName { get; set; } = string.Empty;
}