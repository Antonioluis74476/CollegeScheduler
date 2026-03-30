using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using System.Net.Http.Json;

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

        public async Task<RecurringEventCreateResultDto?> CreateRecurringEventsAsync(RecurringEventCreateDto request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/admin/scheduling/recurring-events",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<RecurringEventCreateResultDto>();
        }

        public async Task<EventStatusDtoPagedResult?> GetEventStatusesAsync()
        {
            return await _httpClient.GetFromJsonAsync<EventStatusDtoPagedResult>(
                "api/v1/admin/event-statuses"
            );
        }

        public async Task<RoomDtoPagedResult?> GetRoomsByBuildingAsync(int buildingId)
        {
            return await _httpClient.GetFromJsonAsync<RoomDtoPagedResult>(
                $"api/v1/admin/buildings/{buildingId}/rooms"
            );
        }

        public async Task<ModuleDtoPagedResult?> GetModulesAsync()
        {
            return await _httpClient.GetFromJsonAsync<ModuleDtoPagedResult>(
                "api/v1/admin/modules"
            );
        }

        public async Task<TermDtoPagedResult?> GetTermsByAcademicYearAsync(int academicYearId)
        {
            return await _httpClient.GetFromJsonAsync<TermDtoPagedResult>(
                $"api/v1/admin/academic-years/{academicYearId}/terms"
            );
        }
    }
}