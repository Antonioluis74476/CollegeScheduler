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

    public int? RoomId { get; set; }

    public string? RoomCode { get; set; }

    public string? RoomName { get; set; }

    public string? BuildingName { get; set; }

    public string? CampusName { get; set; }

    public DateTime? StartUtc { get; set; }

    public DateTime? EndUtc { get; set; }

    public int? ExpectedAttendees { get; set; }

    public string? Purpose { get; set; }

}