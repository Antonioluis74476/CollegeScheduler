namespace CollegeScheduler.DTOs.Lecturer;

public sealed class LecturerProfileDto
{
	public int LecturerId { get; set; }

	public string StaffNumber { get; set; } = "";

	public string Name { get; set; } = "";

	public string LastName { get; set; } = "";

	public string Email { get; set; } = "";

	public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

}