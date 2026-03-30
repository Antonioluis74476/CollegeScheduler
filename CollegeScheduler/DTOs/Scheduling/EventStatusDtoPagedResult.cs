namespace CollegeScheduler.DTOs.Scheduling;

public class EventStatusDtoPagedResult
{
    public List<EventStatusDto>? Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }


}