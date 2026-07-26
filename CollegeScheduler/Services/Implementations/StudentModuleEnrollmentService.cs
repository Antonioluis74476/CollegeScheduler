using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Membership;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public sealed class StudentModuleEnrollmentService
    : IStudentModuleEnrollmentService
{
    private readonly HttpClient _httpClient;

    public StudentModuleEnrollmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<StudentModuleEnrollmentDto>> GetAllAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<
            PagedResult<StudentModuleEnrollmentDto>>(
            "api/v1/admin/student-module-enrollments?page=1&pageSize=100");

        return result?.Items ?? [];
    }

    public async Task<StudentModuleEnrollmentDto?> GetAsync(
        int studentId,
        int moduleId,
        int termId)
    {
        return await _httpClient.GetFromJsonAsync<StudentModuleEnrollmentDto>(
            $"api/v1/admin/student-module-enrollments/" +
            $"{studentId}/{moduleId}/{termId}");
    }

    public async Task<bool> CreateAsync(
        StudentModuleEnrollmentCreateDto dto)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/student-module-enrollments",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(
        int studentId,
        int moduleId,
        int termId,
        StudentModuleEnrollmentUpdateDto dto)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/student-module-enrollments/" +
            $"{studentId}/{moduleId}/{termId}",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(
        int studentId,
        int moduleId,
        int termId)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/student-module-enrollments/" +
            $"{studentId}/{moduleId}/{termId}");

        return response.IsSuccessStatusCode;
    }

    public async Task<IReadOnlyList<StudentModuleEnrollmentDto>>
        GetByStudentAsync(int studentId)
    {
        return await _httpClient.GetFromJsonAsync<
            List<StudentModuleEnrollmentDto>>(
            $"api/v1/admin/students/{studentId}/module-enrollments")
            ?? [];
    }
}