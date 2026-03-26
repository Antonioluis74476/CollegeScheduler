namespace CollegeScheduler.DTOs.Profiles;

public sealed class LecturerDto
{
	public int LecturerId { get; set; }

	public string UserId { get; set; } = "";

	public string Email { get; set; } = "";

	public string StaffNumber { get; set; } = "";

	public string Name { get; set; } = "";

	public string LastName { get; set; } = "";

	public int? DepartmentId { get; set; }

	public string EmploymentType { get; set; } = "";

	public int MaxWeeklyHours { get; set; }

	public bool IsActive { get; set; }

	public DateTime CreatedAtUtc { get; set; }

	public DateTime? UpdatedAtUtc { get; set; }
}
