using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestRoomBookingCreateDto
{
	[Required]
	public int RoomId { get; set; }

	[Required]
	public DateTime StartUtc { get; set; }

	[Required]
	public DateTime EndUtc { get; set; }

	[Required]
	[StringLength(300)]
	public string Purpose { get; set; } = string.Empty;

	[Range(1, int.MaxValue)]
	public int ExpectedAttendees { get; set; } = 1;
}