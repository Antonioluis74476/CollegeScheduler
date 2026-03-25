namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestTypeDto
{
	public int RequestTypeId { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
}