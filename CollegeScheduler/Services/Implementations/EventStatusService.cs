using System.Net;
using System.Net.Http.Json;
using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Admin;

public sealed class EventStatusService : IEventStatusService
{
    private readonly HttpClient _httpClient;

    public EventStatusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CollegeScheduler.DTOs.Common.PagedResult<EventStatusDto>?> GetAllAsync(
    int page = 1,
    int pageSize = 10,
    string? search = null)
    {
        var url = $"api/v1/admin/event-statuses?page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        return await _httpClient.GetFromJsonAsync<
            CollegeScheduler.DTOs.Common.PagedResult<EventStatusDto>>(url);
    }

    public async Task<EventStatusDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/admin/event-statuses/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<EventStatusDto>();
    }

    public async Task<bool> CreateAsync(string name)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/event-statuses",
            new { Name = name });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/event-statuses/{id}",
            new { Name = name });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/event-statuses/{id}");

        return response.IsSuccessStatusCode;
    }
}