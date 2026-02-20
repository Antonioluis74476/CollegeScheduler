using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomUnavailabilityUpdateDto
{
	[Required]
	public DateTime StartUtc { get; init; }

	[Required]
	public DateTime EndUtc { get; init; }

	[Required]
	public int UnavailabilityReasonTypeId { get; init; }

	[StringLength(500)]
	public string? Notes { get; init; }
}
