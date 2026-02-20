namespace CollegeScheduler.DTOs.Academic;

public sealed class CohortModuleQuery
{
	// Searches module (by code/title) - implemented by controller (join)
	public string? Search { get; set; }

	// Filters
	public int? TermId { get; set; }
	public int? ModuleId { get; set; }
	public bool? IsRequired { get; set; }
	public bool? IsActive { get; set; }

	// Sorting (safe whitelist in controller)
	// moduleId / termId / isRequired / createdAt
	public string? SortBy { get; set; } = "moduleId";
	public string? SortDir { get; set; } = "asc";

	// Paging
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
