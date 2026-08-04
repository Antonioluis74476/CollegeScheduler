using System.Net.Http.Json;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly HttpClient _httpClient;

        public RoomTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.RoomTypeDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var url = $"api/v1/admin/room-types?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.RoomTypeDto>>(url);
        }

        public async Task<CollegeScheduler.DTOs.Facilities.RoomTypeDto?> GetByIdAsync(int roomTypeId)
        {
            return await _httpClient.GetFromJsonAsync<CollegeScheduler.DTOs.Facilities.RoomTypeDto>(
                $"api/v1/admin/room-types/{roomTypeId}");
        }

        public async Task<bool> CreateAsync(CollegeScheduler.DTOs.Facilities.RoomTypeCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/room-types", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int roomTypeId, CollegeScheduler.DTOs.Facilities.RoomTypeUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/admin/room-types/{roomTypeId}", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int roomTypeId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/admin/room-types/{roomTypeId}");

            return response.IsSuccessStatusCode;
        }
    }
}