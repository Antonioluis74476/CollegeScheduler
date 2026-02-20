namespace CollegeScheduler.Data.Entities.Common;

public abstract class AuditableEntity
{
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAtUtc { get; set; }
	public bool IsActive { get; set; } = true;
}
