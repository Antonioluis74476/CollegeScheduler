using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;
using System.Net.Http.Json;

public sealed class EventLecturerService : IEventLecturerService
{
    private readonly HttpClient _httpClient;

    public EventLecturerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<EventLecturerDto>> GetAllAsync(
        long? timetableEventId = null,
        int? lecturerId = null,
        int page = 1,
        int pageSize = 20)
    {
        var queryParameters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (timetableEventId.HasValue)
            queryParameters.Add($"timetableEventId={timetableEventId.Value}");

        if (lecturerId.HasValue)
            queryParameters.Add($"lecturerId={lecturerId.Value}");

        var url =
            $"api/v1/admin/event-lecturers?{string.Join("&", queryParameters)}";

        var result =
            await _httpClient.GetFromJsonAsync<PagedResult<EventLecturerDto>>(url);

        return result ?? new PagedResult<EventLecturerDto>
        {
            Items = new List<EventLecturerDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EventLecturerDto> CreateAsync(
        EventLecturerCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/event-lecturers",
            dto);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<EventLecturerDto>();

        return result
            ?? throw new InvalidOperationException(
                "The Event Lecturer assignment was created, but no data was returned.");
    }

    public async Task DeleteAsync(
        long timetableEventId,
        int lecturerId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/event-lecturers/{timetableEventId}/{lecturerId}");

        response.EnsureSuccessStatusCode();
    }
}