using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<PagedResult<CollegeScheduler.DTOs.Facilities.BuildingDto>?> GetByCampusAsync(
            int campusId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CollegeScheduler.DTOs.Facilities.BuildingDto?> GetByIdAsync(int buildingId);

        Task<bool> CreateAsync(int campusId, BuildingCreateDto dto);
        Task<bool> UpdateAsync(int buildingId, BuildingUpdateDto dto);
        Task<bool> DeleteAsync(int buildingId);
    }
}