namespace CollegeScheduler.DTOs.Profiles;

public sealed class StudentDto
{
	public int StudentId { get; set; }

	public string UserId { get; set; } = "";

	public string Email { get; set; } = "";

	public string StudentNumber { get; set; } = "";

	public string Name { get; set; } = "";

	public string LastName { get; set; } = "";

	public string Status { get; set; } = "";

	public bool IsActive { get; set; }

	public DateTime CreatedAtUtc { get; set; }

	public DateTime? UpdatedAtUtc { get; set; }
}
