namespace CollegeScheduler.DTOs.Profiles;

public sealed class LecturerQuery
{
	public string? Search { get; set; }

	public string? StaffNumber { get; set; }

	public int? DepartmentId { get; set; }

	public string? EmploymentType { get; set; }

	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "staffNumber"; // or "name"
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
