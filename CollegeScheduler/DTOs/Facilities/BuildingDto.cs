namespace CollegeScheduler.DTOs.Facilities;

public sealed class BuildingDto
{
	public int BuildingId { get; init; }
	public int CampusId { get; init; }

	public string Code { get; init; } = "";
	public string Name { get; init; } = "";

	public string? Faculty { get; init; }

	public bool IsActive { get; init; }
}
