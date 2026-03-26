namespace CollegeScheduler.DTOs.Academic;

public sealed class ModuleQuery
{
	// Generic search over Code + Title
	public string? Search { get; set; }

	// Exact match filters
	public string? Code { get; set; }
	public int? DepartmentId { get; set; }

	public bool? IsActive { get; set; }

	// Numeric filters
	public int? MinCredits { get; set; }
	public int? MaxCredits { get; set; }
	public int? MinRoomCapacity { get; set; }

	// Sorting
	public string? SortBy { get; set; } = "code"; // code/title/credits/minroomcapacity
	public string? SortDir { get; set; } = "asc";

	// Paging
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
