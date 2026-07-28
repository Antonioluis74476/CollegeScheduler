using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;

public interface IEventLecturerService
{
    Task<PagedResult<EventLecturerDto>> GetAllAsync(
        long? timetableEventId = null,
        int? lecturerId = null,
        int page = 1,
        int pageSize = 20);

    Task<EventLecturerDto> CreateAsync(EventLecturerCreateDto dto);

    Task DeleteAsync(long timetableEventId, int lecturerId);
}