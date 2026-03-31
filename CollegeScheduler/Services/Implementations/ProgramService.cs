using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class ProgramService : IProgramService
    {
        private readonly HttpClient _httpClient;

        public ProgramService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<ProgramDto>?> GetByDepartmentAsync(
            int departmentId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/departments/{departmentId}/programs?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<ProgramDto>>(url);
        }

        public async Task<ProgramDto?> GetByIdAsync(int programId)
        {
            return await _httpClient.GetFromJsonAsync<ProgramDto>(
                $"api/v1/admin/programs/{programId}");
        }

        public async Task<bool> CreateAsync(int departmentId, ProgramCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/departments/{departmentId}/programs", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int programId, ProgramUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/programs/{programId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}