using System.Net.Http.Json;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class RoomFeatureService : IRoomFeatureService
    {
        private readonly HttpClient _httpClient;

        public RoomFeatureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CollegeScheduler.DTOs.Facilities.RoomFeatureDto>?> GetByRoomIdAsync(int roomId)
        {
            return await _httpClient.GetFromJsonAsync<List<CollegeScheduler.DTOs.Facilities.RoomFeatureDto>>(
                $"api/v1/admin/rooms/{roomId}/features");
        }

        public async Task<bool> AddAsync(int roomId, CollegeScheduler.DTOs.Facilities.RoomFeatureAddDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/rooms/{roomId}/features", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int roomId, int featureId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/admin/rooms/{roomId}/features/{featureId}");

            return response.IsSuccessStatusCode;
        }
    }
}