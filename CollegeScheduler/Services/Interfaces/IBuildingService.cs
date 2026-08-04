namespace CollegeScheduler.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.BuildingDto>?> GetByCampusAsync(
            int campusId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CollegeScheduler.DTOs.Facilities.BuildingDto?> GetByIdAsync(int buildingId);

        Task<bool> CreateAsync(int campusId, CollegeScheduler.DTOs.Facilities.BuildingCreateDto dto);

        Task<bool> UpdateAsync(int buildingId, CollegeScheduler.DTOs.Facilities.BuildingUpdateDto dto);

        Task<bool> DeleteAsync(int buildingId);
    }
}