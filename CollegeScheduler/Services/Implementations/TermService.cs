using System.Net.Http.Json;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class TermService : ITermService
    {
        private readonly HttpClient _httpClient;

        public TermService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<TermDto>?> GetByAcademicYearAsync(
            int academicYearId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/academic-years/{academicYearId}/terms?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResult<TermDto>>(url);
        }

        public async Task<TermDto?> GetByIdAsync(int termId)
        {
            return await _httpClient.GetFromJsonAsync<TermDto>(
                $"api/v1/admin/terms/{termId}");
        }

        public async Task<bool> CreateAsync(int academicYearId, TermCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/academic-years/{academicYearId}/terms", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int termId, TermUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/terms/{termId}", dto);

            return response.IsSuccessStatusCode;
        }
    }
}