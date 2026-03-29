using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class BuildingService : IBuildingService
    {
        private readonly HttpClient _httpClient;

        public BuildingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<CollegeScheduler.DTOs.Facilities.BuildingDto>?> GetByCampusAsync(
            int campusId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/campuses/{campusId}/buildings";

            return await _httpClient.GetFromJsonAsync<
                PagedResult<CollegeScheduler.DTOs.Facilities.BuildingDto>>(url);
        }

        public async Task<CollegeScheduler.DTOs.Facilities.BuildingDto?> GetByIdAsync(int buildingId)
        {
            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Facilities.BuildingDto>(
                $"api/v1/admin/buildings/{buildingId}");
        }

        public async Task<bool> CreateAsync(int campusId, BuildingCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/campuses/{campusId}/buildings", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int buildingId, BuildingUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/buildings/{buildingId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int buildingId)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/admin/buildings/{buildingId}");
            return response.IsSuccessStatusCode;
        }
    }
}