using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public class AdminUserService : IAdminUserService
{
    private readonly HttpClient _http;

    public AdminUserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<StudentDto>> GetStudentsAsync()
    {
        var result = await _http.GetFromJsonAsync<PagedResult<StudentDto>>(
            "api/v1/admin/students?page=1&pageSize=100");

        return result?.Items ?? new List<StudentDto>();
    }

    public async Task<StudentDto?> CreateStudentAsync(StudentCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/admin/students", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        return await response.Content.ReadFromJsonAsync<StudentDto>();
    }

    public async Task<List<LecturerDto>> GetLecturersAsync()
    {
        var result = await _http.GetFromJsonAsync<PagedResult<LecturerDto>>(
            "api/v1/admin/lecturers?page=1&pageSize=100");

        return result?.Items ?? new List<LecturerDto>();
    }

    public async Task<LecturerDto?> CreateLecturerAsync(LecturerCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/admin/lecturers", dto);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LecturerDto>();
    }

    public async Task UpdateStudentAsync(int id, StudentUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/admin/students/{id}", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task UpdateLecturerAsync(int id, LecturerUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/admin/lecturers/{id}", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task ActivateStudentAsync(int id)
    {
        var response = await _http.PatchAsync($"api/v1/admin/students/{id}/activate", null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task DeactivateStudentAsync(int id)
    {
        var response = await _http.PatchAsync($"api/v1/admin/students/{id}/deactivate", null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task ActivateLecturerAsync(int id)
    {
        var response = await _http.PatchAsync($"api/v1/admin/lecturers/{id}/activate", null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task DeactivateLecturerAsync(int id)
    {
        var response = await _http.PatchAsync($"api/v1/admin/lecturers/{id}/deactivate", null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }
}