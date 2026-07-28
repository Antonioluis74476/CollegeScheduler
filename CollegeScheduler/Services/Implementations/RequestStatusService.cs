using System.Net;
using System.Net.Http.Json;
using CollegeScheduler.DTOs.Requests;

namespace CollegeScheduler.Services.Admin;

public sealed class RequestStatusService : IRequestStatusService
{
    private readonly HttpClient _httpClient;

    public RequestStatusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CollegeScheduler.DTOs.Common.PagedResult<RequestStatusDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var url = $"api/v1/admin/request-statuses?page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        return await _httpClient.GetFromJsonAsync<
            CollegeScheduler.DTOs.Common.PagedResult<RequestStatusDto>>(url);
    }

    public async Task<RequestStatusDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/admin/request-statuses/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RequestStatusDto>();
    }

    public async Task<bool> CreateAsync(string name)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/request-statuses",
            new
            {
                Name = name
            });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, string name, bool isActive)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/request-statuses/{id}",
            new
            {
                Name = name,
                IsActive = isActive
            });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/request-statuses/{id}");

        return response.IsSuccessStatusCode;
    }
}