using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.DTOs.Requests;


namespace CollegeScheduler.Services.Interfaces
{
    public interface IAdminSchedulingService
    {
        Task<List<AvailableRoomDto>?> GetAvailableRoomsAsync(RoomSearchQuery query);
        Task<ClashResult?> CheckClashesAsync(ClashCheckRequest request);
        Task<RecurringEventCreateResultDto?> CreateRecurringEventsAsync(RecurringEventCreateDto request);
        Task<EventStatusDtoPagedResult?> GetEventStatusesAsync();
        Task<RoomDtoPagedResult?> GetRoomsByBuildingAsync(int buildingId);
        Task<ModuleDtoPagedResult?> GetModulesAsync();
        Task<TermDtoPagedResult?> GetTermsByAcademicYearAsync(int academicYearId);
        Task<PagedResult<CohortDto>?> GetCohortsByProgramAsync(int programId);
        Task<PagedResult<DepartmentDto>?> GetDepartmentsAsync();

        Task<PagedResult<ProgramDto>?> GetProgramsByDepartmentAsync(int departmentId);
        Task<PagedResult<LecturerDto>?> GetLecturersAsync();

        Task<List<PendingRequestDto>?> GetPendingRequestsAsync();

        Task<DecisionResultDto?> DecideRequestAsync(
            long requestId,
            DecideRequestDto request);

        Task<PagedResult<CollegeScheduler.DTOs.Facilities.CampusDto>?> GetCampusesAsync();

        Task<PagedResult<CollegeScheduler.DTOs.Facilities.BuildingDto>?> GetBuildingsByCampusAsync(int campusId);

        Task<RecurringEventSeriesDto?> GetRecurringEventSeriesAsync(Guid recurrenceGroupId);

        Task UpdateRecurringSeriesAsync(
            Guid recurrenceGroupId,
            UpdateRecurringEventDto request);

        Task CancelRecurringSeriesAsync(
            Guid recurrenceGroupId,
            CancelRecurringEventDto request);

    }
}