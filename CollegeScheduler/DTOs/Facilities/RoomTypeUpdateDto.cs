using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomTypeUpdateDto
{
	[Required, StringLength(80)]
	public string Name { get; init; } = "";
}
