namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestQuery
{
	public string? Search { get; set; }
	public int? RequestTypeId { get; set; }
	public int? RequestStatusId { get; set; }
	public string? RequestedByUserId { get; set; }
	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "createdAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}