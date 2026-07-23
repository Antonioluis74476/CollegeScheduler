using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Membership;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public sealed class StudentCohortMembershipService
    : IStudentCohortMembershipService
{
    private readonly HttpClient _httpClient;

    public StudentCohortMembershipService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<StudentCohortMembershipDto>> GetAllAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<
            PagedResult<StudentCohortMembershipDto>>(
            "api/v1/admin/student-cohort-memberships?page=1&pageSize=100");

        return result?.Items ?? [];
    }

    public async Task<StudentCohortMembershipDto?> GetAsync(
        int studentId,
        int cohortId,
        int academicYearId)
    {
        return await _httpClient.GetFromJsonAsync<StudentCohortMembershipDto>(
            $"api/v1/admin/student-cohort-memberships/" +
            $"{studentId}/{cohortId}/{academicYearId}");
    }

    public async Task<bool> CreateAsync(
        StudentCohortMembershipCreateDto dto)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/admin/student-cohort-memberships",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(
        int studentId,
        int cohortId,
        int academicYearId,
        StudentCohortMembershipUpdateDto dto)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/admin/student-cohort-memberships/" +
            $"{studentId}/{cohortId}/{academicYearId}",
            dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(
        int studentId,
        int cohortId,
        int academicYearId)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/v1/admin/student-cohort-memberships/" +
            $"{studentId}/{cohortId}/{academicYearId}");

        return response.IsSuccessStatusCode;
    }

    public async Task<IReadOnlyList<StudentCohortMembershipDto>>
        GetByStudentAsync(int studentId)
    {
        return await _httpClient.GetFromJsonAsync<
            List<StudentCohortMembershipDto>>(
            $"api/v1/admin/students/{studentId}/cohort-memberships")
            ?? [];
    }
}