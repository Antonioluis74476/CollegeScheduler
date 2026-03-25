namespace CollegeScheduler.DTOs.Audit;

public sealed class AuditLogDto
{
	public long AuditLogId { get; set; }
	public string? UserId { get; set; }
	public string Action { get; set; } = string.Empty;
	public string EntityType { get; set; } = string.Empty;
	public string? EntityId { get; set; }
	public string? OldValuesJson { get; set; }
	public string? NewValuesJson { get; set; }
	public string? IpAddress { get; set; }
	public string? UserAgent { get; set; }
	public DateTime PerformedAtUtc { get; set; }
}