using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Interfaces;

public interface ITimetableEventService
{
    Task<PagedResult<TimetableEventDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 100);

    Task<TimetableEventDto?> GetByIdAsync(long id);

    Task<bool> CreateAsync(TimetableEventCreateDto dto);

    Task<bool> UpdateAsync(
        long id,
        TimetableEventUpdateDto dto);

    Task<bool> DeleteAsync(long id);
}