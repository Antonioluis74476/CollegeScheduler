namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestDecisionDto
{
	public long RequestDecisionId { get; set; }
	public long RequestId { get; set; }
	public string DecidedByUserId { get; set; } = string.Empty;
	public string Decision { get; set; } = string.Empty;
	public string? Comment { get; set; }
	public DateTime DecidedAtUtc { get; set; }
	public bool IsActive { get; set; }
}