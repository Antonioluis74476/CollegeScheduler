using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HttpClient _httpClient;

        public DepartmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<DepartmentDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/departments?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<DepartmentDto>>(url);
        }

        public async Task<DepartmentDto?> GetByIdAsync(int departmentId)
        {
            return await _httpClient.GetFromJsonAsync<DepartmentDto>(
                $"api/v1/admin/departments/{departmentId}");
        }

        public async Task<bool> CreateAsync(DepartmentCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/departments", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int departmentId, DepartmentUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/departments/{departmentId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}