using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class ModuleService : IModuleService
    {
        private readonly HttpClient _httpClient;

        public ModuleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<ModuleDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/modules?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<ModuleDto>>(url);
        }

        public async Task<ModuleDto?> GetByIdAsync(int moduleId)
        {
            return await _httpClient.GetFromJsonAsync<ModuleDto>(
                $"api/v1/admin/modules/{moduleId}");
        }

        public async Task<bool> CreateAsync(ModuleCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/modules", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int moduleId, ModuleUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/modules/{moduleId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}