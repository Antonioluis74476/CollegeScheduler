using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class CohortService : ICohortService
    {
        private readonly HttpClient _httpClient;

        public CohortService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<CohortDto>?> GetByProgramAsync(
            int programId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/programs/{programId}/cohorts?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<CohortDto>>(url);
        }

        public async Task<CohortDto?> GetByIdAsync(int cohortId)
        {
            return await _httpClient.GetFromJsonAsync<CohortDto>(
                $"api/v1/admin/cohorts/{cohortId}");
        }

        public async Task<bool> CreateAsync(int programId, CohortCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/programs/{programId}/cohorts", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int cohortId, CohortUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/cohorts/{cohortId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}