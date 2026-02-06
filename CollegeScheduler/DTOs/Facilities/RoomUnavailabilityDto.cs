namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomUnavailabilityDto
{
	public int RoomUnavailabilityId { get; init; }
	public int RoomId { get; init; }

	public DateTime StartUtc { get; init; }
	public DateTime EndUtc { get; init; }

	public int UnavailabilityReasonTypeId { get; init; }
	public string? ReasonName { get; init; }

	public string? Notes { get; init; }

	public string CreatedByUserId { get; init; } = "";
	public DateTime CreatedAtUtc { get; init; }
	public DateTime? UpdatedAtUtc { get; init; }
}
