namespace CollegeScheduler.DTOs.Audit;

public sealed class AuditLogQuery
{
	public string? UserId { get; set; }
	public string? Action { get; set; }
	public string? EntityType { get; set; }
	public string? EntityId { get; set; }

	public string? SortBy { get; set; } = "performedAt";
	public string? SortDir { get; set; } = "desc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}