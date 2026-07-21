using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Scheduling;


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
        Task<PagedResult<ProgramDto>?> GetProgramsAsync();
        Task<PagedResult<LecturerDto>?> GetLecturersAsync();
    }
}