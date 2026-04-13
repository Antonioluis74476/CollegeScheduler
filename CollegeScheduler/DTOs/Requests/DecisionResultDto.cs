namespace CollegeScheduler.DTOs.Requests;

public sealed class DecisionResultDto
{
	public bool IsSuccess { get; set; }
	public string Message { get; set; } = string.Empty;
}