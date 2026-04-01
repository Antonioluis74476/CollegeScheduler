using CollegeScheduler.DTOs.Facilities;

namespace CollegeScheduler.Services.Interfaces
{
    public interface ICampusService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CampusDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CampusDto?> GetByIdAsync(int campusId);
        Task<bool> CreateAsync(CampusCreateDto dto);
        Task<bool> UpdateAsync(int campusId, CampusUpdateDto dto);
        Task<bool> DeleteAsync(int campusId);
    }
}