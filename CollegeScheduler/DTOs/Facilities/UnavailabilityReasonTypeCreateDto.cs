using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class UnavailabilityReasonTypeCreateDto
{
	[Required, StringLength(100)]
	public string Name { get; init; } = "";
}
