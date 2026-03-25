namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestDecisionQuery
{
	public long? RequestId { get; set; }
	public string? DecidedByUserId { get; set; }
	public string? Decision { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "decidedAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}