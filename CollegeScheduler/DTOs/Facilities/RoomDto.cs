namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomDto
{
	public int RoomId { get; init; }
	public int BuildingId { get; init; }
	public int RoomTypeId { get; init; }

	public string Code { get; init; } = "";
	public string? Name { get; init; }
	public string? Floor { get; init; }

	public int Capacity { get; init; }
	public bool IsBookableByStudents { get; init; }
	public bool RequiresApproval { get; init; }
	public bool IsActive { get; init; }

	public string? Notes { get; init; }
}
