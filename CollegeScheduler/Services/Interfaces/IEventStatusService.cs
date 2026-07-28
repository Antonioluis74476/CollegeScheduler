using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Admin;

public interface IEventStatusService
{
    Task<CollegeScheduler.DTOs.Common.PagedResult<EventStatusDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null);
    Task<EventStatusDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(string name);
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);
}