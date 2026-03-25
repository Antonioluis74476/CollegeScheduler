using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestCreateDto
{
	[Required]
	public int RequestTypeId { get; set; }

	[Required]
	public int RequestStatusId { get; set; }

	[Required]
	[StringLength(450)]
	public string RequestedByUserId { get; set; } = string.Empty;

	[StringLength(200)]
	public string? Title { get; set; }

	public string? Notes { get; set; }

	public RequestRoomBookingCreateDto? RoomBookingDetail { get; set; }
	public RequestScheduleChangeCreateDto? ScheduleChangeDetail { get; set; }
}