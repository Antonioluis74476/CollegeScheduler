using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public class ModuleLecturerService : IModuleLecturerService
{
    private readonly HttpClient _httpClient;

    public ModuleLecturerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<ModuleLecturerDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 100)
    {
        return await _httpClient
            .GetFromJsonAsync<PagedResult<ModuleLecturerDto>>(
                $"api/v1/admin/module-lecturers?page={page}&pageSize={pageSize}");
    }

    public async Task<ModuleLecturerDto?> GetByIdAsync(
        int moduleId,
        int lecturerId,
        int termId)
    {
        return await _httpClient
            .GetFromJsonAsync<ModuleLecturerDto>(
                $"api/v1/admin/module-lecturers/" +
                $"{moduleId}/{lecturerId}/{termId}");
    }

    public async Task<bool> CreateAsync(
        ModuleLecturerCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/module-lecturers",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(
        int moduleId,
        int lecturerId,
        int termId,
        ModuleLecturerUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/module-lecturers/" +
            $"{moduleId}/{lecturerId}/{termId}",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(
        int moduleId,
        int lecturerId,
        int termId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/module-lecturers/" +
            $"{moduleId}/{lecturerId}/{termId}");

        return response.IsSuccessStatusCode;
    }
}