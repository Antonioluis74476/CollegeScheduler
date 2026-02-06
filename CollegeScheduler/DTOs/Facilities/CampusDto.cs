namespace CollegeScheduler.DTOs.Facilities;

public sealed class CampusDto
{
	public int CampusId { get; init; }
	public string Code { get; init; } = "";
	public string Name { get; init; } = "";
	public string? Address { get; init; }
	public string? City { get; init; }
	public bool IsActive { get; init; }
}
