using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public class TimetableEventService : ITimetableEventService
{
    private readonly HttpClient _httpClient;

    public TimetableEventService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<TimetableEventDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 100)
    {
        return await _httpClient
            .GetFromJsonAsync<PagedResult<TimetableEventDto>>(
                $"api/v1/admin/timetable-events?page={page}&pageSize={pageSize}");
    }

    public async Task<TimetableEventDto?> GetByIdAsync(long id)
    {
        return await _httpClient
            .GetFromJsonAsync<TimetableEventDto>(
                $"api/v1/admin/timetable-events/{id}");
    }

    public async Task<bool> CreateAsync(TimetableEventCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/timetable-events",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(
        long id,
        TimetableEventUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/timetable-events/{id}",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/timetable-events/{id}");

        return response.IsSuccessStatusCode;
    }
}