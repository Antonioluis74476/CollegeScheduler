using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly HttpClient _httpClient;

        public RoomService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<CollegeScheduler.DTOs.Facilities.RoomDto>?> GetByBuildingAsync(
            int buildingId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/buildings/{buildingId}/rooms?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResult<CollegeScheduler.DTOs.Facilities.RoomDto>>(url);
        }

        public async Task<CollegeScheduler.DTOs.Facilities.RoomDto?> GetByIdAsync(int roomId)
        {
            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Facilities.RoomDto>(
                $"api/v1/admin/rooms/{roomId}");
        }

        public async Task<bool> CreateAsync(int buildingId, RoomCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/buildings/{buildingId}/rooms", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int roomId, RoomUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/rooms/{roomId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int roomId)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/admin/rooms/{roomId}");
            return response.IsSuccessStatusCode;
        }
    }
}