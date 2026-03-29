using System.Net.Http.Json;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations
{
    public class AdminSchedulingService : IAdminSchedulingService
    {
        private readonly HttpClient _httpClient;

        public AdminSchedulingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AvailableRoomDto>?> GetAvailableRoomsAsync(RoomSearchQuery query)
        {
            var queryParts = new List<string>
            {
                $"startUtc={Uri.EscapeDataString(query.StartUtc.ToString("o"))}",
                $"endUtc={Uri.EscapeDataString(query.EndUtc.ToString("o"))}"
            };

            if (query.MinCapacity.HasValue)
                queryParts.Add($"minCapacity={query.MinCapacity.Value}");

            if (query.RoomTypeId.HasValue)
                queryParts.Add($"roomTypeId={query.RoomTypeId.Value}");

            if (query.BuildingId.HasValue)
                queryParts.Add($"buildingId={query.BuildingId.Value}");

            if (query.CampusId.HasValue)
                queryParts.Add($"campusId={query.CampusId.Value}");

            if (query.RequiredFeatureIds is { Count: > 0 })
            {
                foreach (var featureId in query.RequiredFeatureIds)
                {
                    queryParts.Add($"requiredFeatureIds={featureId}");
                }
            }

            var url = $"api/v1/admin/scheduling/rooms/available?{string.Join("&", queryParts)}";

            return await _httpClient.GetFromJsonAsync<List<AvailableRoomDto>>(url);
        }

        public async Task<ClashResult?> CheckClashesAsync(ClashCheckRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/scheduling/check-clashes",
                request
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ClashResult>();
        }
    }
}