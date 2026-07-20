using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;

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

            var response = await _httpClient.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode || body.TrimStart().StartsWith("<"))
            {
                throw new Exception(
                    $"API returned non-JSON. Status: {response.StatusCode}\n\n{body.Substring(0, Math.Min(body.Length, 500))}");
            }

            return await response.Content.ReadFromJsonAsync<List<AvailableRoomDto>>();
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

        /*public async Task<EventStatusDtoPagedResult?> GetEventStatusesAsync()
        {
            return await _httpClient.GetFromJsonAsync<EventStatusDtoPagedResult>(
                "api/v1/admin/event-statuses"
            );
        }*/

        public async Task<EventStatusDtoPagedResult?> GetEventStatusesAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/admin/event-statuses");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode || body.TrimStart().StartsWith("<"))
            {
                throw new Exception(
                    $"API returned non-JSON. Status: {response.StatusCode}. Body: {body.Substring(0, Math.Min(body.Length, 500))}"
                );
            }

            return await response.Content.ReadFromJsonAsync<EventStatusDtoPagedResult>();
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

        public async Task<PagedResult<CohortDto>?> GetCohortsByProgramAsync(int programId)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<CohortDto>>(
                $"api/v1/admin/programs/{programId}/cohorts?page=1&pageSize=100"
            );
        }

        public async Task<PagedResult<ProgramDto>?> GetProgramsAsync()
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<ProgramDto>>(
                "api/v1/admin/departments/1/programs?page=1&pageSize=100"
            );
        }

        public async Task<PagedResult<LecturerDto>?> GetLecturersAsync()
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<LecturerDto>>(
                "api/v1/admin/lecturers?page=1&pageSize=100"
            );
        }

    

    public async Task<List<PendingRequestDto>?> GetPendingRequestsAsync()
        {
            var response = await _httpClient.GetAsync(
                "api/v1/admin/scheduling/requests/pending"
            );

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unable to load pending requests. " +
                    $"Status: {response.StatusCode}. Response: {body}"
                );
            }

            return await response.Content
                .ReadFromJsonAsync<List<PendingRequestDto>>();
        }

        public async Task<DecisionResultDto?> DecideRequestAsync(
            long requestId,
            DecideRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/admin/scheduling/requests/{requestId}/decide",
                request
            );

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unable to process request. " +
                    $"Status: {response.StatusCode}. Response: {body}"
                );
            }

            return await response.Content
                .ReadFromJsonAsync<DecisionResultDto>();
        }
    }
}