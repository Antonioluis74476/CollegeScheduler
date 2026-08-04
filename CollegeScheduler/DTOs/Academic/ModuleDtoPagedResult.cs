namespace CollegeScheduler.DTOs.Academic;

public class ModuleDtoPagedResult
{
    public List<ModuleDto>? Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}