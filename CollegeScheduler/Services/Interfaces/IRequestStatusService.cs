using CollegeScheduler.DTOs.Requests;

namespace CollegeScheduler.Services.Admin;

public interface IRequestStatusService
{
    Task<CollegeScheduler.DTOs.Common.PagedResult<RequestStatusDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null);

    Task<RequestStatusDto?> GetByIdAsync(int id);

    Task<bool> CreateAsync(string name);

    Task<bool> UpdateAsync(int id, string name, bool isActive);

    Task<bool> DeleteAsync(int id);
}