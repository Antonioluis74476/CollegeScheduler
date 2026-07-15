namespace CollegeScheduler.DTOs.Student;

public sealed class StudentRequestDto
{
	public long RequestId { get; set; }

	public string Title { get; set; } = "";

	public string? Notes { get; set; }

	public string RequestType { get; set; } = "";

	public string RequestStatus { get; set; } = "";

	public DateTime CreatedAtUtc { get; set; }

	public DateTime? UpdatedAtUtc { get; set; }
}