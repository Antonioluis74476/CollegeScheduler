using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Interfaces;

public interface IEventCohortService
{
    Task<PagedResult<EventCohortDto>?> GetAllAsync(
        long? timetableEventId = null,
        int? cohortId = null,
        int page = 1,
        int pageSize = 100);

    Task<EventCohortDto?> CreateAsync(
        EventCohortCreateDto dto);

    Task<bool> DeleteAsync(
        long timetableEventId,
        int cohortId);
}