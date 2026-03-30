namespace CollegeScheduler.DTOs.Academic;

public class TermDtoPagedResult
{
    public List<TermDto>? Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}