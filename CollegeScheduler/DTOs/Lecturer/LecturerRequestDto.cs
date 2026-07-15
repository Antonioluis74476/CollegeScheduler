namespace CollegeScheduler.DTOs.Lecturer;

public sealed class LecturerRequestDto
{
	public long RequestId { get; set; }

	public string Title { get; set; } = "";

	public string? Notes { get; set; }

	public string RequestType { get; set; } = "";

	public string RequestStatus { get; set; } = "";

	public DateTime CreatedAtUtc { get; set; }

	public DateTime? UpdatedAtUtc { get; set; }
}