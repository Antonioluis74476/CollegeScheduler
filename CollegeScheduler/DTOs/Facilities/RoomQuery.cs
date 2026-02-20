namespace CollegeScheduler.DTOs.Facilities;

public sealed class RoomQuery
{
	public string? Search { get; init; }
	public string? Code { get; init; }
	public int? RoomTypeId { get; init; }
	public bool? IsActive { get; init; }
	public bool? IsBookableByStudents { get; init; }
	public bool? RequiresApproval { get; init; }
	public int? MinCapacity { get; init; }

	public int Page { get; init; } = 1;
	public int PageSize { get; init; } = 20;

	public string SortBy { get; init; } = "code"; // code/name/capacity
	public string SortDir { get; init; } = "asc";
}
