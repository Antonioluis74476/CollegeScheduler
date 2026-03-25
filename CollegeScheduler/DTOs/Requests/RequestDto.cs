namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestDto
{
	public long RequestId { get; set; }
	public int RequestTypeId { get; set; }
	public int RequestStatusId { get; set; }
	public string RequestedByUserId { get; set; } = string.Empty;
	public string? Title { get; set; }
	public string? Notes { get; set; }
	public bool IsActive { get; set; }

	public RequestRoomBookingDto? RoomBookingDetail { get; set; }
	public RequestScheduleChangeDto? ScheduleChangeDetail { get; set; }
}