using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomFeatureAddDto
{
	[Required]
	public int FeatureId { get; init; }
}
