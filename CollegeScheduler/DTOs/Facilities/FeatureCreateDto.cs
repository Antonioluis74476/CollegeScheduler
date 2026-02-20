using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class FeatureCreateDto
{
	[Required, StringLength(80)]
	public string Name { get; init; } = "";
}
