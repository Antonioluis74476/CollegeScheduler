namespace CollegeScheduler.DTOs.Requests;

public sealed class RoomBookingRequestCreateDto
{
	public int RoomId { get; set; }
	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }
	public string Purpose { get; set; } = string.Empty;
	public int ExpectedAttendees { get; set; }
}