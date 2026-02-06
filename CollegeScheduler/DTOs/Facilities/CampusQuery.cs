namespace CollegeScheduler.DTOs.Facilities
{
	public sealed class CampusQuery
	{
		public string? Search { get; init; }          // “contains” search
		public string? Code { get; init; }            // exact
		public string? City { get; init; }            // exact
		public bool? IsActive { get; init; }          // optional filter

		public int Page { get; init; } = 1;
		public int PageSize { get; init; } = 20;

		public string SortBy { get; init; } = "name"; // name, code, city, createdAt
		public string SortDir { get; init; } = "asc"; // asc/desc
	}

}
