namespace CollegeScheduler.DTOs.Profiles;

public sealed class StudentQuery
{
	public string? Search { get; set; }

	public string? StudentNumber { get; set; }

	public string? Status { get; set; }

	public bool? IsActive { get; set; }

	public string? SortBy { get; set; } = "studentNumber"; // or "name"
	public string? SortDir { get; set; } = "asc";

	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
