using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly HttpClient _httpClient;

        public AcademicYearService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<AcademicYearDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/academic-years?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResult<AcademicYearDto>>(url);
        }

        public async Task<AcademicYearDto?> GetByIdAsync(int academicYearId)
        {
            return await _httpClient.GetFromJsonAsync<AcademicYearDto>(
                $"api/v1/admin/academic-years/{academicYearId}");
        }

        public async Task<bool> CreateAsync(AcademicYearCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/academic-years", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int academicYearId, AcademicYearUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/academic-years/{academicYearId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetCurrentAsync(int academicYearId)
        {
            var response = await _httpClient.PatchAsync(
                $"api/v1/admin/academic-years/{academicYearId}/set-current",
                null);

            return response.IsSuccessStatusCode;
        }
    }
}