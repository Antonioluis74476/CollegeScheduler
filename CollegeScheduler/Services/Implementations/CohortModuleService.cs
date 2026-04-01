using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class CohortModuleService : ICohortModuleService
    {
        private readonly HttpClient _httpClient;

        public CohortModuleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<CohortModuleDto>?> GetByCohortAsync(
            int cohortId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/cohorts/{cohortId}/modules?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<CohortModuleDto>>(url);
        }

        public async Task<CohortModuleDto?> GetByIdAsync(int cohortModuleId)
        {
            return await _httpClient.GetFromJsonAsync<CohortModuleDto>(
                $"api/v1/admin/cohort-modules/{cohortModuleId}");
        }

        public async Task<bool> CreateAsync(int cohortId, CohortModuleCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/cohorts/{cohortId}/modules", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int cohortModuleId, CohortModuleUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/cohort-modules/{cohortModuleId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}