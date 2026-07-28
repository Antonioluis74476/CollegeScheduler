using System.Net.Http.Json;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Services.Implementations;

public class RequestTypeService : IRequestTypeService
{
    private readonly HttpClient _http;

    public RequestTypeService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RequestTypeDto>> GetAllAsync()
    {
        var result =
            await _http.GetFromJsonAsync<PagedResult<RequestTypeDto>>(
                "api/v1/admin/request-types");

        return result?.Items ?? new List<RequestTypeDto>();
    }

    public async Task<RequestTypeDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<RequestTypeDto>(
            $"api/v1/admin/request-types/{id}");
    }

    public async Task<bool> CreateAsync(RequestTypeCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/admin/request-types",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, RequestTypeUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync(
            $"api/v1/admin/request-types/{id}",
            dto);

        return response.IsSuccessStatusCode;
    }
}