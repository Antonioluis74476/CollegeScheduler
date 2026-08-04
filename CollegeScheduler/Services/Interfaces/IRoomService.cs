namespace CollegeScheduler.Services.Interfaces
{
    public interface IRoomService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.RoomDto>?> GetByBuildingAsync(
            int buildingId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CollegeScheduler.DTOs.Facilities.RoomDto?> GetByIdAsync(int roomId);

        Task<bool> CreateAsync(int buildingId, CollegeScheduler.DTOs.Facilities.RoomCreateDto dto);

        Task<bool> UpdateAsync(int roomId, CollegeScheduler.DTOs.Facilities.RoomUpdateDto dto);

        Task<bool> DeleteAsync(int roomId);
    }
}