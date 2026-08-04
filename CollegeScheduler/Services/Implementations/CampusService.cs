using System.Net.Http.Json;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class CampusService : ICampusService
    {
        private readonly HttpClient _httpClient;

        public CampusService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<CampusDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/campuses?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<CampusDto>>(url);
        }

        public async Task<CampusDto?> GetByIdAsync(int campusId)
        {
            return await _httpClient.GetFromJsonAsync<CampusDto>(
                $"api/v1/admin/campuses/{campusId}");
        }

        public async Task<bool> CreateAsync(CampusCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/campuses", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int campusId, CampusUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/campuses/{campusId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int campusId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/admin/campuses/{campusId}");

            return response.IsSuccessStatusCode;
        }
    }
}