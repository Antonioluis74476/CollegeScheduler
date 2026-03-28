namespace CollegeScheduler.DTOs.Requests;

public sealed class DecideRequestDto
{
	public string Decision { get; set; } = string.Empty; // Approved or Rejected
	public string? Comment { get; set; }
}