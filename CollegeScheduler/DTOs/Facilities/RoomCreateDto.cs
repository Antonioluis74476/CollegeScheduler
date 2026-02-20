using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomCreateDto
{
	[Required, StringLength(30)]
	public string Code { get; init; } = "";

	[StringLength(200)]
	public string? Name { get; init; }

	[StringLength(50)]
	public string? Floor { get; init; }

	[Range(0, 10000)]
	public int Capacity { get; init; } = 0;

	public bool IsBookableByStudents { get; init; } = false;
	public bool RequiresApproval { get; init; } = true;

	[StringLength(500)]
	public string? Notes { get; init; }

	[Range(1, int.MaxValue)]
	public int RoomTypeId { get; init; }
}
