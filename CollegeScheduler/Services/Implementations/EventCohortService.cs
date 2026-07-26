using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public class EventCohortService : IEventCohortService
{
    private readonly HttpClient _httpClient;

    public EventCohortService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<EventCohortDto>?> GetAllAsync(
        long? timetableEventId = null,
        int? cohortId = null,
        int page = 1,
        int pageSize = 100)
    {
        var url =
            $"api/v1/admin/event-cohorts?page={page}&pageSize={pageSize}";

        if (timetableEventId.HasValue)
            url += $"&timetableEventId={timetableEventId.Value}";

        if (cohortId.HasValue)
            url += $"&cohortId={cohortId.Value}";

        return await _httpClient.GetFromJsonAsync<PagedResult<EventCohortDto>>(url);
    }

    public async Task<EventCohortDto?> CreateAsync(
        EventCohortCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/event-cohorts",
            dto);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EventCohortDto>();
    }

    public async Task<bool> DeleteAsync(
        long timetableEventId,
        int cohortId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/event-cohorts/{timetableEventId}/{cohortId}");

        return response.IsSuccessStatusCode;
    }
}