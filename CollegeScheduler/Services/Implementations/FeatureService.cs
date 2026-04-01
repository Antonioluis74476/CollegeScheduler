using System.Net.Http.Json;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class FeatureService : IFeatureService
    {
        private readonly HttpClient _httpClient;

        public FeatureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.FeatureDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/features?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.FeatureDto>>(url);
        }

        public async Task<CollegeScheduler.DTOs.Facilities.FeatureDto?> GetByIdAsync(int featureId)
        {
            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Facilities.FeatureDto>(
                $"api/v1/admin/features/{featureId}");
        }

        public async Task<bool> CreateAsync(CollegeScheduler.DTOs.Facilities.FeatureCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/features", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int featureId, CollegeScheduler.DTOs.Facilities.FeatureUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/features/{featureId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int featureId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/admin/features/{featureId}");

            return response.IsSuccessStatusCode;
        }
    }
}